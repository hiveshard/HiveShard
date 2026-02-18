using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Client.Interfaces;
using HiveShard.Data;
using HiveShard.Edge.Events;
using HiveShard.Edge.Interfaces;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Providers;

namespace HiveShard.Fabrics.InMemory;

public class InMemoryEdgeFabric: IEdgeTunnelClientEndpoint, IEdgeTunnelServerEndpoint
{
    private readonly IHiveShardTelemetry _debugLoggingProvider;
    private readonly IAddressProvider _addressProvider;
    private readonly Dictionary<Type, Action<object>> _clientCallbacks = new();
    private readonly Dictionary<Type, Action<object, HiveShardClient>> _serverCallbacks = new();

    private HiveShardClient _connectedHiveShardClient = null;
    private Action<HiveShardClient> _clientConnectedCallback = client => { };

    public InMemoryEdgeFabric(IHiveShardTelemetry debugLoggingProvider, IAddressProvider addressProvider)
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
}