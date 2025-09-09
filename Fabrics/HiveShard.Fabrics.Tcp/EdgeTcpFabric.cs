using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Edge.events;
using HiveShard.Fabric.Edge;
using HiveShard.Fabrics.Tcp.Data;
using HiveShard.Interface;

namespace HiveShard.Fabrics.Tcp
{
    public class EdgeTcpFabric: IEdgeTunnelServerEndpoint
    {
        private TcpListener _tcpListener;
        private ConcurrentDictionary<HiveShard.Data.Client, ConnectedClient> _boundClients = new ConcurrentDictionary<HiveShard.Data.Client, ConnectedClient>();
        private ConcurrentDictionary<TcpClient, ConnectedClient> _unboundClients = new ConcurrentDictionary<TcpClient, ConnectedClient>();
        private Dictionary<string, Action<string, HiveShard.Data.Client>> _eventRegistrations = new();
        private IAddressProvider _edgeIdentityProvider;
        private ISerializer _serializer;
        private Action<Client> _clientConnectedCallback = client => { };
        private volatile int _ready;

        public EdgeTcpFabric(IAddressProvider edgeIdentityProvider, ISerializer serializer, INetworkConfiguration networkConfiguration)
        {
            _serializer = serializer;
            _edgeIdentityProvider = edgeIdentityProvider;
            _tcpListener = TcpListener.Create(networkConfiguration.Port());
        }

        public Task Start(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                _tcpListener.Start();
                Interlocked.Increment(ref _ready);
                while (!token.IsCancellationRequested)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    var cancellationTokenSource = new CancellationTokenSource();
                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, token);
                    var stream = tcpClient.GetStream();
                    var connectedClient = new ConnectedClient(tcpClient, linkedTokenSource, stream);
                    HandleClient(connectedClient, linkedTokenSource.Token);
                    
                    _unboundClients.AddOrUpdate(tcpClient, _ => connectedClient, (_, _) => connectedClient);
                }
            }, token);
        }

        public async Task WaitForReady() {
            while (_ready < 1)
            {
                await Task.Delay(100);
            }
        }

        private void SendTcpMessage(object message, Type messageType, NetworkStream stream)
        {
            var tcpMessage = new TcpMessage(messageType.AssemblyQualifiedName, _serializer.Serialize(message));
            var json = _serializer.Serialize(tcpMessage);
            var payload = Encoding.UTF8.GetBytes(json);

            var lengthPrefix = BitConverter.GetBytes(payload.Length);
            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(payload, 0, payload.Length);
        }
        
        private async Task<TcpMessage> ConsumeTcpStream(NetworkStream stream)
        {
            byte[] lengthBuffer = new byte[4];
            int readSoFar = 0;
            while (readSoFar < 4)
            {
                int bytesRead = await stream.ReadAsync(lengthBuffer, readSoFar, 4 - readSoFar);
                if (bytesRead == 0) throw new Exception("Connection closed while reading length prefix");
                readSoFar += bytesRead;
            }
            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] buffer = new byte[messageLength];
            int totalRead = 0;
            while (totalRead < messageLength)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalRead, messageLength - totalRead);
                if (bytesRead == 0) throw new Exception("Connection closed");
                totalRead += bytesRead;
            }

            var receivedText = Encoding.UTF8.GetString(buffer, 0, totalRead);
            return _serializer.Deserialize<TcpMessage>(receivedText);
        } 
    
        private void HandleClient(ConnectedClient connectedClient, CancellationToken token)
        {
            _ = Task.Run(async () =>
            {
                HiveShard.Data.Client client = null;
                var tcpClient = connectedClient.TcpClient;
                var stream = connectedClient.Stream;
                try
                {
                    var connectionSucceeded = new ConnectionSucceeded(_edgeIdentityProvider.GetUri());
                    SendTcpMessage(connectionSucceeded, typeof(ConnectionSucceeded), stream);

                    var clientLoginTcpMessage = await ConsumeTcpStream(stream);
                    var tryBindToEdge = clientLoginTcpMessage.GetExpectedPayload<EdgeBindingRequest>(_serializer);
                    client = tryBindToEdge.Client;
                    _boundClients.AddOrUpdate(client, _ => connectedClient, (_, _) => connectedClient);
                    _unboundClients.TryRemove(tcpClient, out _);

                    var edgeBoundNotification = new EdgeBoundNotification(_edgeIdentityProvider.GetUri());
                    SendTcpMessage(edgeBoundNotification, typeof(EdgeBoundNotification), stream);

                    while (true)
                    {
                        var tcpMessage = await ConsumeTcpStream(stream);
                        Console.WriteLine($"Consumed Message: {tcpMessage.TypeFullName()}, Contents: {tcpMessage.Payload}");
                        if(tcpMessage.TypeFullName() == typeof(EdgeUnbindingRequest).FullName)
                            break;
                        
                        _eventRegistrations[tcpMessage.TypeFullName()](tcpMessage.Payload, client);
                    }
                }
                catch (Exception e)
                {
                    Exception exception;
                    if(client is null)
                    {
                        exception = new Exception("Failure with unknown client", e);
                        Console.WriteLine(exception);
                        throw exception;
                    }
                    exception = new Exception($"Failure with client: {client.Username}", e);
                    Console.WriteLine(exception);
                    throw exception;
                }
                finally
                {
                    if(client is not null)
                        _boundClients.TryRemove(client, out _);
                    _unboundClients.TryRemove(tcpClient, out _);
                    
                    var remoteEndPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                    tcpClient.Client.Shutdown(SocketShutdown.Send);
                    tcpClient.Close();
                    if(client is null)
                    {
                        Console.WriteLine($"Unknown client disconnected: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                    }
                    else
                    {
                        Console.WriteLine($"Client disconnected: {client.Username}, {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                    }
                }
            }, token);
        }

        public void SendEvent(object message, Type messageType, Client client)
        {
            if (!_boundClients.TryGetValue(client, out ConnectedClient connectedClient))
                throw new Exception("client was not bound yet");

            SendTcpMessage(message, messageType, connectedClient.Stream);
        }

        public void RegisterCallback(Action<object, Client> callback, Type type)
        {
            var typeFullName = type.FullName;
            if (typeFullName is null)
                throw new Exception("");
            _eventRegistrations[typeFullName] = (message, client) =>
            {
                var deserializedMessage = _serializer.Deserialize(message,  type);
                if (deserializedMessage is null)
                    throw new Exception("Deserialization didnt work");
                callback(deserializedMessage, client);
            };
        }

        public void RegisterClientConnectedCallback(Action<Client> handler)
        {
            _clientConnectedCallback = handler;
        }
    }
}