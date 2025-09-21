using System;
using System.Threading.Tasks;

namespace HiveShard.Fabric.Client
{
    public interface IEdgeTunnelClientEndpoint: IFabric
    {
        Task SendEvent(object message, Type messageType);

        void RegisterCallback(Action<object> callback, Type type);
        void Disconnect(Data.HiveShardClient hiveShardClient);
        Task Connect(Data.HiveShardClient hiveShardClient);
    }
}