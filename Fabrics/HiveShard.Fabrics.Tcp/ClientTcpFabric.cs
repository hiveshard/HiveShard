using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Edge.events;
using HiveShard.Fabric;
using HiveShard.Fabric.Client;
using HiveShard.Fabrics.Tcp.Data;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Util;

namespace HiveShard.Fabrics.Tcp
{
    public class ClientTcpFabric: IEdgeTunnelClientEndpoint
    {
        private TcpClient _tcpClient;

        public ClientTcpFabric(ISerializer serializer, INetworkConfiguration networkConfiguration, ICancellationProvider cancellationProvider, IFabricLoggingProvider fabricLoggingProvider, IIdentityConfig identityConfig)
        {
            _networkConfiguration = networkConfiguration;
            _cancellationProvider = cancellationProvider;
            _serializer = serializer;
            _scopedLogger = fabricLoggingProvider.GetScopedLogger<ClientTcpFabric>(identityConfig);
            _tcpClient = new TcpClient();
        }
        
        private Dictionary<Type, ConcurrentQueue<object>> _messages = new Dictionary<Type, ConcurrentQueue<object>>();
        private Dictionary<Type, ISet<Action<object>>> _handlers = new Dictionary<Type, ISet<Action<object>>>();
        private NetworkStream _networkStream;
        private ISerializer _serializer;
        private ICancellationProvider _cancellationProvider;

        private volatile int _ready;
        private INetworkConfiguration _networkConfiguration;
        private IScopedFabricLoggingProvider _scopedLogger;

        public Task Start(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                Interlocked.Increment(ref _ready);
                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (var pair in _messages)
                    {
                        var type = pair.Key;
                        var queue = pair.Value;

                        while (!queue.IsEmpty)
                        {
                            if (queue.TryDequeue(out object result))
                            {
                                foreach (var action in _handlers[type])
                                {
                                    action(result);
                                }
                            }
                        }
                    }

                    await Task.Delay(100);
                }
            }, cancellationToken);
            
            return Task.CompletedTask;
        }

        public async Task WaitForReady() {
            while (_ready < 1)
            {
                await Task.Delay(100);
            }
        }

        private async Task SendTcpMessage(object message, Type messageType, NetworkStream stream)
        {
            var serializedPayload = _serializer.Serialize(message);
            var tcpMessage = new TcpMessage(messageType.AssemblyQualifiedName, serializedPayload);
            var json = _serializer.Serialize(tcpMessage);
            var messageBytes = Encoding.UTF8.GetBytes(json);

            var lengthPrefix = BitConverter.GetBytes(messageBytes.Length);
            await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }

        public void Disconnect(HiveShardClient hiveShardClient)
        {
            var edgeUnbindingRequest = new EdgeUnbindingRequest(hiveShardClient);
            if (_networkStream is not null)
            {
                SendTcpMessage(edgeUnbindingRequest, typeof(EdgeUnbindingRequest), _networkStream)
                    .Wait();
                _networkStream.Flush();
            
                _tcpClient.Client.Shutdown(SocketShutdown.Send);
            
                var buffer = new byte[1024];
                while (_networkStream.Read(buffer, 0, buffer.Length) > 0) { }
            }
            _tcpClient.Close();
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
        public async Task Connect(HiveShard.Data.HiveShardClient hiveShardClient)
        {
            await Resilience.Retry(async token =>
            {
                await _tcpClient.ConnectAsync("localhost", _networkConfiguration.Port());
            }, "Tcp client connection retry", _cancellationProvider.GetToken(), _scopedLogger);
            _networkStream = _tcpClient.GetStream();
            
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var tcpMessage = await ConsumeTcpStream(_networkStream);
                    var type = Type.GetType(tcpMessage.AssemblyType);
                    if (type is null)
                        throw new Exception("type did not exist");
                    
                    if (!_messages.ContainsKey(type))
                        throw new Exception($"message was not expected: {type.AssemblyQualifiedName}");
                    Console.WriteLine($"Received message of type {type.FullName}");
                    var payload = _serializer.Deserialize(tcpMessage.Payload, type);
                    _messages[type].Enqueue(payload);
                }
            });
        }

        public async Task SendEvent(object message, Type messageType)
        {
            await SendTcpMessage(message, messageType, _networkStream);
        }

        public void RegisterCallback(Action<object> callback, Type type)
        {
            if (_messages.ContainsKey(type))
            {
                Console.WriteLine($"type already existed: {type.AssemblyQualifiedName}");
                return;
            }
            _messages[type] = new ConcurrentQueue<object>();
            _handlers[type] = new HashSet<Action<object>>();
            _handlers[type].Add(callback);
        }
    }
}