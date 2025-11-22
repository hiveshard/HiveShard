using HiveShard;
using HiveShard.Config;
using HiveShard.Data;
using HiveShard.Fabric.Ticker;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Workers.Edge;
using HiveShard.Workers.Edge.Data;
using InMemory.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace InMemory;

public class InMemoryDeployment: IDeployment
{
    public ServiceEnvironment Build(int gridSize, IEnumerable<WorkerDefinition> workers)
    {
        List<WorkerEnvironment> workerEnvironments = new List<WorkerEnvironment>();

        IServiceCollection topLevelServices = new ServiceCollection()
            .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "test"))
            .AddSingleton<IHiveShardSimpleLoggingProvider, SimpleLoggingProvider>()
            .AddSingleton<ITelemetryProvider, SimpleTelemetryProvider>()
            .AddSingleton<IFabricLoggingProvider, FabricLoggingProvider>()
            .AddSingleton<IShardRepository, ShardRepository>()
            .AddSingleton<ISerializer, NewtonsoftSerializer>()
            .AddSingleton<ITickRepository, TickRepository>()
            .AddSingleton<ICancellationProvider, CancellationProvider>()
            .AddSingleton<IWorkerLoggingProvider, SimpleLoggingProvider>()
            .AddSingleton<ISimpleFabric, InMemorySimpleFabric>();
        
        
        foreach (var workerDefinition in workers)
        {
            if (workerDefinition is EdgeWorkerDefinition edgeWorkerEnvironment)
            {
                workerEnvironments.Add(BuildEdgeWorker(edgeWorkerEnvironment));
            }
        }

        return new ServiceEnvironment(gridSize, topLevelServices, workerEnvironments);
    }

    private WorkerEnvironment BuildEdgeWorker(EdgeWorkerDefinition edgeWorkerDefinition)
    {
        ServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<EdgeWorker>();
        return new WorkerEnvironment(serviceCollection, [typeof(IIdentityConfig)]);
    }
}