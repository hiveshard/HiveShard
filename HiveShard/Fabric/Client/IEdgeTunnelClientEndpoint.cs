using System;
using System.Threading.Tasks;

namespace HiveShard.Fabric.Client
{
    public interface IEdgeTunnelClientEndpoint: IFabric
    {
        Task SendEvent(object message, Type messageType);

        void RegisterCallback(Action<object> callback, Type type);
        void Disconnect(Data.Client client);
        Task Connect(Data.Client client);
    }
}