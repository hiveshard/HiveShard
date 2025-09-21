using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Edge.events;
using HiveShard.Fabric.Client;
using HiveShard.Fabric.Edge;
using HiveShard.Interface;
using HiveShard.Interface.Logging;

namespace HiveShard.Fabrics.InMemory
{
    public class InMemoryEdgeFabric: IEdgeTunnelClientEndpoint, IEdgeTunnelServerEndpoint
    {
        private IDebugLoggingProvider _debugLoggingProvider;
        private IAddressProvider _addressProvider;
        private Dictionary<Type, Action<object>> _clientCallbacks = new Dictionary<Type, Action<object>>();
        private Dictionary<Type, Action<object, HiveShardClient>> _serverCallbacks = new Dictionary<Type, Action<object, HiveShardClient>>();

        private HiveShardClient _connectedHiveShardClient = null;
        private Action<HiveShardClient> _clientConnectedCallback = client => { };

        public InMemoryEdgeFabric(IDebugLoggingProvider debugLoggingProvider, IAddressProvider addressProvider)
        {
            _debugLoggingProvider = debugLoggingProvider;
            _addressProvider = addressProvider;
            _serverCallbacks[typeof(EdgeBindingRequest)] = (o, c) =>
            {
                SendEvent(new EdgeBoundNotification(_addressProvider.GetUri()), typeof(EdgeBoundNotification), c);
            };
        }

        public Task SendEvent(object message, Type messageType)
        {
            _serverCallbacks[messageType](message, _connectedHiveShardClient);
            return Task.CompletedTask;
        }

        public void RegisterCallback(Action<object> callback, Type type)
        {
            _clientCallbacks[type] = callback;
        }

        public void Disconnect(HiveShardClient hiveShardClient)
        {
            _connectedHiveShardClient = null;
            _debugLoggingProvider.LogDebug($"Client disconnected from edge");
        }

        public Task Start(CancellationToken cancellationToken)
        {
            _debugLoggingProvider.LogDebug($"{nameof(InMemoryEdgeFabric)} started");
            return Task.CompletedTask;
        }

        public Task Connect(HiveShardClient hiveShardClient)
        {
            _connectedHiveShardClient = hiveShardClient;
            _debugLoggingProvider.LogDebug($"Client connected to edge");
            _clientConnectedCallback(hiveShardClient);
            SendEvent(new ConnectionSucceeded(_addressProvider.GetUri()), typeof(ConnectionSucceeded), hiveShardClient);
            return Task.CompletedTask;
        }

        public void SendEvent(object message, Type messageType, HiveShardClient hiveShardClient)
        {
            _clientCallbacks[messageType](message);
        }

        public void RegisterCallback(Action<object, HiveShardClient> callback, Type type)
        {
            _serverCallbacks[type] = callback;
        }

        public void RegisterClientConnectedCallback(Action<HiveShardClient> handler)
        {
            _clientConnectedCallback = handler;
        }

        public Task WaitForReady() => Task.CompletedTask;
    }
}