using HiveShard;
using HiveShard.Client;
using HiveShard.Client.Data;
using HiveShard.Client.Interface;
using HiveShard.Config;
using HiveShard.Data;
using HiveShard.Edge;
using HiveShard.Fabric.Client;
using HiveShard.Fabric.Edge;
using HiveShard.Fabric.Ticker;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Util;
using HiveShard.Workers.Edge;
using HiveShard.Workers.Edge.Data;
using InMemory.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace InMemory;

public class InMemoryDeployment: IDeployment
{
    private List<CompartmentEnvironment> _isolatedEnvironments = new();

    public ServiceEnvironment Build(int gridSize, IEnumerable<IsolatedEnvironment> workers)
    {
        CancellationProvider cancellationProvider = new CancellationProvider();
        IServiceCollection topLevelServices = new ServiceCollection()
            .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "test"))
            .AddSingleton<IHiveShardSimpleLoggingProvider, SimpleLoggingProvider>()
            .AddSingleton<ITelemetryProvider, SimpleTelemetryProvider>()
            .AddSingleton<IFabricLoggingProvider, FabricLoggingProvider>()
            .AddSingleton<IShardRepository, ShardRepository>()
            .AddSingleton<ISerializer, NewtonsoftSerializer>()
            .AddSingleton<ITickRepository, TickRepository>()
            .AddSingleton<ICancellationProvider>(cancellationProvider)
            .AddSingleton<CancellationProvider>(cancellationProvider)
            .AddSingleton<IWorkerLoggingProvider, SimpleLoggingProvider>()
            .AddSingleton<ISimpleFabric, InMemorySimpleFabric>()
            .AddSingleton<IDebugLoggingProvider, SimpleLoggingProvider>()
            .AddSingleton<InMemoryEdgeFabric>()
            .AddSingleton<IEdgeTunnelClientEndpoint>(x => x.GetRequiredService<InMemoryEdgeFabric>())
            .AddSingleton<IEdgeTunnelServerEndpoint>(x => x.GetRequiredService<InMemoryEdgeFabric>())
            .AddSingleton<IAddressProvider, EdgeIdentityProvider>();
        
        
        foreach (var isolatedEnvironment in workers)
        {
            if (isolatedEnvironment is EdgeWorkerIsolatedEnvironment edgeWorkerEnvironment)
                BuildEdgeWorker(edgeWorkerEnvironment);
            if(isolatedEnvironment is ClientIsolatedEnvironment clientIsolatedEnvironment)
                BuildClient(clientIsolatedEnvironment);
        }

        return new ServiceEnvironment(gridSize, topLevelServices, _isolatedEnvironments);
    }

    private void BuildEdgeWorker(EdgeWorkerIsolatedEnvironment edgeWorkerIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<EdgeWorker>();
        serviceCollection.AddSingleton<IEdgeTunnel, EdgeTunnel>();
        var compartmentEnvironment = new CompartmentEnvironment(
            $"edge-{edgeWorkerIsolatedEnvironment.Identifier}", 
            serviceCollection, 
            new DependencyBuilder()
                .Add<CancellationProvider>()
                .Add<ICancellationProvider>()
                .Add<IAddressProvider>()
                .Add<IIdentityConfig>()
                .Add<IEdgeTunnelServerEndpoint>()
                .Build()
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }

    private void BuildClient(ClientIsolatedEnvironment clientIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IClientTunnel, ClientTunnel>();
        serviceCollection.AddSingleton<HiveShardClient>(new HiveShardClient(clientIsolatedEnvironment.Username));
        var compartmentEnvironment = new CompartmentEnvironment(
            $"client-{clientIsolatedEnvironment.Username}", 
            serviceCollection, 
            new DependencyBuilder()
                .Add<IEdgeTunnelClientEndpoint>()
                .Add<ICancellationProvider>()
                .Add<IDebugLoggingProvider>()
                .Build()
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }

}