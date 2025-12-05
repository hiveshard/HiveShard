using System;
using System.Threading.Tasks;

namespace HiveShard.Fabric.Edge
{
    public interface IEdgeTunnelServerEndpoint: IFabric
    {
        public void SendEvent(object message, Type messageType, Data.HiveShardClient hiveShardClient);

        public void RegisterCallback(Action<object, Data.HiveShardClient> callback, Type type);
        void RegisterClientConnectedCallback(Action<Data.HiveShardClient> handler);
    }
}