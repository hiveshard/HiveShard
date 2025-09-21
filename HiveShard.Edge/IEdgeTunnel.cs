using System;
using System.Threading.Tasks;
using HiveShard.Data;

namespace HiveShard.Edge
{
    public interface IEdgeTunnel
    {
        void RegisterEdgeHandler<TEvent>(Action<TEvent, HiveShardClient> handler);
        void SendEdgeEventToClient<TEvent>(TEvent @event, HiveShardClient hiveShardClient);
        void SetClientConnectedCallback(Action<HiveShardClient> handler);
        Task Start();
    }
}