using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Client;
using HiveShard.Client.Data;
using HiveShard.Client.Interfaces;
using HiveShard.Config;
using HiveShard.Data;
using HiveShard.Edge;
using HiveShard.Edge.Interfaces;
using HiveShard.Fabrics.InMemory;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Providers;
using HiveShard.Interface.Repository;
using HiveShard.Provider;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Telemetry.Console;
using HiveShard.Util;
using HiveShard.Workers.Edge;
using HiveShard.Workers.Edge.Data;
using HiveShard.Workers.Initializer;
using HiveShard.Workers.Initializer.Data;
using HiveShard.Workers.Initializer.Repositories;
using HiveShard.Workers.Shard;
using HiveShard.Workers.Shard.Data;
using HiveShard.Workers.Shard.Repositories;
using HiveShard.Workers.Ticker;
using HiveShard.Workers.Ticker.Data;
using HiveShard.Workers.Ticker.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Deployments.InMemory;

public class InMemoryDeployment: IDeployment
{
    private readonly List<CompartmentEnvironment> _isolatedEnvironments = new();
    private readonly List<(string, Type, string)> _entryPointLocations = new();

    private void AddEntryPoint<T>(string compartment, string compartmentType)
    where T: class
    {
        _entryPointLocations.Add((compartment, typeof(T), compartmentType));
    }

    public ServiceEnvironment Build(Chunk minChunk, Chunk maxChunk, 
        IEnumerable<IsolatedEnvironment> workers,
        IEventRepository eventRepository)
    {
        var globalChunkConfig = new GlobalChunkConfig(minChunk, maxChunk);
        CancellationProvider cancellationProvider = new CancellationProvider();
        IServiceCollection topLevelServices = new ServiceCollection()
            .AddSingleton<GlobalChunkConfig>(globalChunkConfig)
            .AddSingleton<IEventRepository>(eventRepository)
            .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "test"))
            .AddSingleton<ISerializer, NewtonsoftSerializer>()
            .AddSingleton<ITickRepository, TickRepository>()
            .AddSingleton<ICancellationProvider>(cancellationProvider)
            .AddSingleton(cancellationProvider)
            .AddSingleton<IHiveShardTelemetry, SimpleConsoleTelemetry>()
            .AddSingleton<ISimpleFabric, InMemorySimpleFabric>()
            .AddSingleton<InMemoryEdgeFabric>()
            .AddSingleton<IEdgeTunnelClientEndpoint>(x => x.GetRequiredService<InMemoryEdgeFabric>())
            .AddSingleton<IEdgeTunnelServerEndpoint>(x => x.GetRequiredService<InMemoryEdgeFabric>())
            .AddSingleton<IAddressProvider, EdgeIdentityProvider>();
        
        
        foreach (var isolatedEnvironment in workers)
        {
            if (isolatedEnvironment is EdgeWorkerIsolatedEnvironment edgeWorkerEnvironment)
                BuildEdgeWorker(edgeWorkerEnvironment);
            else if(isolatedEnvironment is ClientIsolatedEnvironment clientIsolatedEnvironment)
                BuildClient(clientIsolatedEnvironment);
            else if (isolatedEnvironment is TickerWorkerIsolatedEnvironment tickerWorkerIsolatedEnvironment)
                BuildTickerWorker(tickerWorkerIsolatedEnvironment);
            else if (isolatedEnvironment is ShardWorkerIsolatedEnvironment shardWorkerIsolatedEnvironment)
                BuildShardWorker(shardWorkerIsolatedEnvironment);
            else if (isolatedEnvironment is InitializerIsolatedEnvironment initializationIsolatedEnvironment)
                BuildInitializationWorker(initializationIsolatedEnvironment);
            else
                throw new NotImplementedException(
                    $"SubEnvironment of type {isolatedEnvironment.GetType()} not implemented");
        }

        return new ServiceEnvironment(globalChunkConfig, topLevelServices, _isolatedEnvironments, _entryPointLocations.AsEnumerable(), eventRepository);
    }

    private void BuildInitializationWorker(InitializerIsolatedEnvironment initializationIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<Initialization>();
        serviceCollection.AddSingleton<InitializationTunnel>();
        InitializerAdditionRepository initializerAdditionRepository = new InitializerAdditionRepository();
        serviceCollection.AddSingleton(initializerAdditionRepository);
        
        foreach (var initializer in initializationIsolatedEnvironment.Initializers)
        {
            initializerAdditionRepository.AddInitializer(initializer);
        }
        
        var compartmentEnvironment = new CompartmentEnvironment(
            $"initializer", 
            serviceCollection, 
            new DependencyBuilder()
                .Add<ISimpleFabric>()
                .Build(),
            typeof(Initialization)
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }

    private void BuildShardWorker(ShardWorkerIsolatedEnvironment shardWorkerIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ShardWorker>();
        serviceCollection.AddSingleton<ITickRepository, TickRepository>();
        serviceCollection.AddSingleton<HiveShardRepository>();
        ShardAdditionRepository repository = new ShardAdditionRepository();
        serviceCollection.AddSingleton<ShardAdditionRepository>(repository);
        
        foreach (var hiveShardIdentity in shardWorkerIsolatedEnvironment.HiveShards)
        {
            repository.Add(new ShardAdditionRequest(hiveShardIdentity));
        }
        
        var compartmentEnvironment = new CompartmentEnvironment(
            $"shardWorker-{shardWorkerIsolatedEnvironment.Identifier}", 
            serviceCollection, 
            new DependencyBuilder()
                .Add<ISimpleFabric>()
                .Add<IHiveShardTelemetry>()
                .Add<ICancellationProvider>()
                .Add<IEventRepository>()
                .Add<ISerializer>()
                .Add<GlobalChunkConfig>()
                .Build(),
            typeof(ShardWorker)
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }
    
    private void BuildEdgeWorker(EdgeWorkerIsolatedEnvironment edgeWorkerIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<EdgeWorker>();
        serviceCollection.AddSingleton<IEdgeTunnel, EdgeTunnel>();
        var compartmentEnvironment = new CompartmentEnvironment(
            $"edgeWorker-{edgeWorkerIsolatedEnvironment.Identifier}", 
            serviceCollection, 
            new DependencyBuilder()
                .Add<CancellationProvider>()
                .Add<ICancellationProvider>()
                .Add<IAddressProvider>()
                .Add<IIdentityConfig>()
                .Add<IEdgeTunnelServerEndpoint>()
                .Build(),
            typeof(EdgeWorker)
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }

    private void BuildClient(ClientIsolatedEnvironment clientIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<ClientTunnel>();
        serviceCollection.AddSingleton<IClientTunnel, ClientTunnel>(x => x.GetRequiredService<ClientTunnel>());
        serviceCollection.AddSingleton<HiveShardClient>(new HiveShardClient(clientIsolatedEnvironment.Username));
        var compartmentEnvironment = new CompartmentEnvironment(
            $"client-{clientIsolatedEnvironment.Username}", 
            serviceCollection, 
            new DependencyBuilder()
                .Add<IEdgeTunnelClientEndpoint>()
                .Add<ICancellationProvider>()
                .Add<IHiveShardTelemetry>()
                .Build(),
            typeof(ClientTunnel)
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }

    private void BuildTickerWorker(TickerWorkerIsolatedEnvironment tickerWorkerIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddSingleton<TickerWorker>();
        serviceCollection.AddSingleton<TickerRepository>();
        var tickerAdditionRepository = new TickerAdditionRepository();
        serviceCollection.AddSingleton<TickerAdditionRepository>(tickerAdditionRepository);
        foreach (var tickerIsolatedEnvironment in tickerWorkerIsolatedEnvironment.Tickers)
        {
            tickerAdditionRepository.RequestEventTickerAddition(tickerIsolatedEnvironment.Identity);
        }
        foreach (var globalTickerIsolatedEnvironment in tickerWorkerIsolatedEnvironment.GlobalTickers)
        {
            tickerAdditionRepository.RequestGlobalTickerAddition(globalTickerIsolatedEnvironment.GlobalTickerIdentity);
        }
        AddEntryPoint<TickerWorker>(tickerWorkerIsolatedEnvironment.TickerWorkerIdentifier, "tickerWorker");

        var compartmentEnvironment = new CompartmentEnvironment(
            $"tickerWorker-{tickerWorkerIsolatedEnvironment.TickerWorkerIdentifier}",
            serviceCollection,
            new DependencyBuilder()
                .Add<ICancellationProvider>()
                .Add<ISimpleFabric>()
                .Add<IEventRepository>()
                .Add<IHiveShardTelemetry>()
                .Add<ServiceEnvironment>()
                .Build(),
            typeof(TickerWorker)
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }
}