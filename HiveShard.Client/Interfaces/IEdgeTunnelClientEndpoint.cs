using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Client.Interfaces;

public interface IEdgeTunnelClientEndpoint: IFabric
{
    Task SendEvent(object message, Type messageType);

    void RegisterCallback(Action<object> callback, Type type);
    void Disconnect(HiveShardClient hiveShardClient);
    Task Connect(HiveShardClient hiveShardClient);
}