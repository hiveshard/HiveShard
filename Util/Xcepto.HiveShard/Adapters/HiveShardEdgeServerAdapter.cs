using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Edge;
using HiveShard.Edge.Interfaces;
using HiveShard.Provider;
using HiveShard.Workers.Edge;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.Adapters;
using Xcepto.HiveShard.States;
using Xcepto.Repositories;

namespace Xcepto.HiveShard.Adapters;

public class HiveShardEdgeServerAdapter<T>: XceptoAdapter
    where T: BaseEdge
{
    private readonly CompartmentIdentifier _compartmentIdentifier;

    public HiveShardEdgeServerAdapter(Guid edgeWorkerName)
    {
        _compartmentIdentifier = new CompartmentIdentifier(edgeWorkerName, CompartmentType.EdgeWorker);
    }
    protected override Task Initialize(IServiceProvider serviceProvider) => Task.CompletedTask;

    protected override Task Cleanup(IServiceProvider serviceProvider)
    {
        var compartmentRepository = serviceProvider.GetRequiredService<CompartmentRepository>();
        var compartment = compartmentRepository.GetCompartment(_compartmentIdentifier.ToString());
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