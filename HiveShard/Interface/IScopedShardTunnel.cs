using System;
using System.Threading.Tasks;
using HiveShard.Fabric;

namespace HiveShard.Interface
{
    public interface IScopedShardTunnel: IFabric, IIsolatedEntryPoint
    {
        public Task Register<TEvent>(Action<TEvent> handler);
        public Task Send<TEvent>(TEvent message);
    }
}