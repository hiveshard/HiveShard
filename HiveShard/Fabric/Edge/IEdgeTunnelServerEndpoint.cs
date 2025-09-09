using System;
using System.Threading.Tasks;

namespace HiveShard.Fabric.Edge
{
    public interface IEdgeTunnelServerEndpoint: IFabric
    {
        public void SendEvent(object message, Type messageType, Data.Client client);

        public void RegisterCallback(Action<object, Data.Client> callback, Type type);
        void RegisterClientConnectedCallback(Action<Data.Client> handler);
    }
}