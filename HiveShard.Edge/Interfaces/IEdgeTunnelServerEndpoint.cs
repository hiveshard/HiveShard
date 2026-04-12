using System;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Edge.Interfaces;

public interface IEdgeTunnelServerEndpoint: IFabric
{
    public void SendEvent(object message, Type messageType, HiveShardClient hiveShardClient);

    public void RegisterCallback(Action<object, HiveShardClient> callback, Type type);
    void RegisterClientConnectedCallback(Action<HiveShardClient> handler);
}