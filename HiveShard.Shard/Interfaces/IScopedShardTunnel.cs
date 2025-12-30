using System;
using System.Threading.Tasks;
using HiveShard.Interface;

namespace HiveShard.Shard.Interfaces
{
    public interface IScopedShardTunnel: IFabric, IIsolatedEntryPoint
    {
        public Task Register<TEvent>(Action<TEvent> handler) where TEvent: IEvent;
        public Task Send<TEvent>(TEvent message) where TEvent: IEvent;
    }
}