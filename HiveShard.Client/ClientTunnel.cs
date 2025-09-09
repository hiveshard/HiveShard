using System;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using HiveShard.Fabric;
using HiveShard.Fabric.Client;
using HiveShard.Interface;

namespace HiveShard.Client
{
    public class ClientTunnel: IClientTunnel
    {
        private IEdgeTunnelClientEndpoint _edgeClient;
        private ICancellationProvider _cancellationProvider;
        private Data.Client _client;

        public ClientTunnel(IEdgeTunnelClientEndpoint edgeClient, Data.Client client, ICancellationProvider cancellationProvider)
        {
            _client = client;
            _cancellationProvider = cancellationProvider;
            _edgeClient = edgeClient;
        }

        public Task RegisterHotPathEventHandler<TEvent>(Action<TEvent> handler)
        {
            _edgeClient.RegisterCallback(e => handler((TEvent)e), typeof(TEvent));
            return Task.CompletedTask;
        }

        public void RegisterLocalEventHandler<TEvent>(Action<TEvent> handler)
        {
            throw new NotImplementedException();
        }

        public void SendHotPathEvent(Type messageType, object message)
        {
            _edgeClient.SendEvent(message, messageType);
        }

        public void SendLocalEvent(Type messageType, object message)
        {
            throw new NotImplementedException();
        }

        public Task SendHotPathEvent<TType>(TType message)
        {
            return _edgeClient.SendEvent(message, typeof(TType));
        }

        public void SendLocalEvent<TType>(TType message)
        {
            throw new NotImplementedException();
        }

        public async Task Connect(Data.Client client)
        {
            await _edgeClient.Start(_cancellationProvider.GetToken());
            await _edgeClient.WaitForReady();
            await _edgeClient.Connect(client);
        }

        public bool TryConsume<T>(out T message)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            _edgeClient.Disconnect(_client);
        }
    }
}