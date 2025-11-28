using System;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using HiveShard.Data;
using HiveShard.Fabric;
using HiveShard.Fabric.Client;
using HiveShard.Interface;

namespace HiveShard.Client
{
    public class ClientTunnel: IClientTunnel, IIsolatedEntryPoint
    {
        private IEdgeTunnelClientEndpoint _edgeClient;
        private ICancellationProvider _cancellationProvider;
        private HiveShardClient _hiveShardClient;

        public ClientTunnel(IEdgeTunnelClientEndpoint edgeClient, HiveShardClient hiveShardClient, ICancellationProvider cancellationProvider)
        {
            _hiveShardClient = hiveShardClient;
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

        public async Task Connect(HiveShardClient hiveShardClient)
        {
            await _edgeClient.Connect(hiveShardClient);
        }

        public bool TryConsume<T>(out T message)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            _edgeClient.Disconnect(_hiveShardClient);
        }

        public Task Start() => Task.CompletedTask;
    }
}