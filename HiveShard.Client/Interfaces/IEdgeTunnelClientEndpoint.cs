using System;
using System.Threading.Tasks;
using HiveShard.Interface;

namespace HiveShard.Client.Interfaces
{
    public interface IEdgeTunnelClientEndpoint: IFabric
    {
        Task SendEvent(object message, Type messageType);

        void RegisterCallback(Action<object> callback, Type type);
        void Disconnect(HiveShard.Data.HiveShardClient hiveShardClient);
        Task Connect(HiveShard.Data.HiveShardClient hiveShardClient);
    }
}