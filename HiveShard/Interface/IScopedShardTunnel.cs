using System;
using System.Threading.Tasks;
using HiveShard.Fabric;

namespace HiveShard.Interface
{
    public interface IScopedShardTunnel: IFabric
    {
        public Task Register<TEvent>(Action<TEvent> handler);
        public void Send<TEvent>(TEvent message);
        void Initialize<T>(T hiveShard) where T : class, IHiveShard;
    }
}