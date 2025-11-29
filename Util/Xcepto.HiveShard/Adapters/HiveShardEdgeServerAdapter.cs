using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Edge;
using HiveShard.Provider;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.States;
using Xcepto.Repositories;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardEdgeServerAdapter<T>: XceptoAdapter
    where T: BaseEdge
    {
        private string _compartmentIdentifier;

        public HiveShardEdgeServerAdapter(string edgeWorkerName)
        {
            _compartmentIdentifier = $"edgeWorker-{edgeWorkerName}";
        }
        protected override Task Initialize(IServiceProvider serviceProvider) => Task.CompletedTask;

        protected override Task Cleanup(IServiceProvider serviceProvider)
        {
            var compartmentRepository = serviceProvider.GetRequiredService<CompartmentRepository>();
            var compartment = compartmentRepository.GetCompartment(_compartmentIdentifier);
            var cancellationProvider = compartment.Services.GetRequiredService<CancellationProvider>();
            cancellationProvider.RequestCancellation();
            return Task.CompletedTask;
        }

        public void Action(Action<IEdgeTunnel> clientAction)
        {
            AddStep(new CompartmentalizedServiceBasedActionState<IEdgeTunnel>("Edge tunnel action", _compartmentIdentifier, x =>
            {
                clientAction(x);
                return Task.CompletedTask;
            })); 
        }
    }
}