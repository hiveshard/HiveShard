using System;
using System.Threading.Tasks;
using HiveShard.Data;

namespace HiveShard.Edge
{
    public interface IEdgeTunnel
    {
        void RegisterEdgeHandler<TEvent>(Action<TEvent, Data.Client> handler);
        void SendEdgeEventToClient<TEvent>(TEvent @event, Data.Client client);
        void SetClientConnectedCallback(Action<Client> handler);
        Task Start();
    }
}