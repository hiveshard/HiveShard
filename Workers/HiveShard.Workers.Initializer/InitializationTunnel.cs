using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Fabric.Ticker;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;

namespace HiveShard.Workers.Initializer
{
    public class InitializationTunnel: IInitializationTunnel
    {
        private ISimpleFabric _simpleFabric;

        public InitializationTunnel(ISimpleFabric simpleFabric)
        {
            _simpleFabric = simpleFabric;
        }

        public void Send<TEvent>(TEvent e, Chunk chunk) where TEvent : IEvent
        {
            
        }

        public async Task FinalizeInitialization()
        {
            throw new System.NotImplementedException();
        }
    }
}