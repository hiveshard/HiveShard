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
using HiveShard.Environment;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Providers;
using HiveShard.Interface.Repository;
using HiveShard.Provider;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Telemetry.Console;
using HiveShard.Telemetry.HiveShardEE;
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
    private readonly List<CompartmentEnvironment> _isolatedEnvironments = [];

    
    public ServiceEnvironment Build(Chunk minChunk, Chunk maxChunk,
        IEnumerable<IsolatedEnvironment> workers,
        IEventRepository eventRepository, string environmentName, ValidationMode validationMode)
    {
        TelemetryConfig telemetryConfig;
        try
        {
            telemetryConfig = new TelemetryConfig(
                new Uri(HiveShardEnv.GetEnv("HIVESHARD_TELEMETRY_ENDPOINT")),
                HiveShardEnv.GetEnv("HIVESHARD_TELEMETRY_TOKEN"),
                HiveShardEnv.GetEnv("HIVESHARD_TELEMETRY_ORGANIZATION"),
                HiveShardEnv.GetEnv("HIVESHARD_TELEMETRY_PROJECT"),
                $"{HiveShardEnv.GetEnv("HIVESHARD_TELEMETRY_ENVIRONMENT_TYPE")}_{environmentName}"
            );
        }
        catch (Exception e)
        {
            Console.WriteLine("[Telemetry WARNING]: optional telemetry not possible: "+ e.Message);
            // dummy object
            telemetryConfig = new TelemetryConfig(new Uri("https://hiveshard.massivecreationlab.com"), "", "", "", "");
        }

        
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
            .AddSingleton<IHiveShardTelemetry, HiveShardEETelemetry>()
            .AddSingleton<TelemetryConfig>(telemetryConfig)
            .AddSingleton<ISimpleFabric, InMemorySimpleFabric>()
            .AddSingleton<InMemoryEdgeFabric>()
            .AddSingleton<IEdgeTunnelClientEndpoint>(x => x.GetRequiredService<InMemoryEdgeFabric>())
            .AddSingleton<IEdgeTunnelServerEndpoint>(x => x.GetRequiredService<InMemoryEdgeFabric>())
            .AddSingleton<IAddressProvider, EdgeIdentityProvider>();
        
        foreach (var isolatedEnvironment in workers)
            switch (isolatedEnvironment)
            {
                case EdgeWorkerIsolatedEnvironment edgeWorkerEnvironment:
                    BuildEdgeWorker(edgeWorkerEnvironment);
                    break;
                case ClientIsolatedEnvironment clientIsolatedEnvironment:
                    BuildClient(clientIsolatedEnvironment);
                    break;
                case TickerWorkerIsolatedEnvironment tickerWorkerIsolatedEnvironment:
                    BuildTickerWorker(tickerWorkerIsolatedEnvironment);
                    break;
                case ShardWorkerIsolatedEnvironment shardWorkerIsolatedEnvironment:
                    BuildShardWorker(shardWorkerIsolatedEnvironment);
                    break;
                case InitializerIsolatedEnvironment initializationIsolatedEnvironment:
                    BuildInitializationWorker(initializationIsolatedEnvironment);
                    break;
                default:
                    throw new NotImplementedException(
                        $"SubEnvironment of type {isolatedEnvironment.GetType()} not implemented");
            }

        var compartmentIdentifier = new CompartmentIdentifier(Guid.NewGuid(), CompartmentType.Root);
        CompartmentEnvironment outer = new CompartmentEnvironment(
            compartmentIdentifier, 
            topLevelServices, 
            [], 
            typeof(ISimpleFabric),
            []
        );

        return new ServiceEnvironment(globalChunkConfig, outer, _isolatedEnvironments, eventRepository, validationMode);
    }

    private void BuildInitializationWorker(InitializerIsolatedEnvironment initializationIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<Initialization>();
        serviceCollection.AddSingleton<InitializationTunnel>();
        InitializerAdditionRepository initializerAdditionRepository = new InitializerAdditionRepository();
        serviceCollection.AddSingleton(initializerAdditionRepository);
        
        foreach (var initializer in initializationIsolatedEnvironment.Initializers) 
            initializerAdditionRepository.AddInitializer(initializer);

        var compartmentEnvironment = new CompartmentEnvironment(
            new CompartmentIdentifier(Guid.NewGuid(), CompartmentType.Initializer), 
            serviceCollection, 
            new DependencyBuilder()
                .Add<ISimpleFabric>()
                .Add<GlobalChunkConfig>()
                .Add<IEventRepository>()
                .Build(),
            typeof(Initialization),
            initializationIsolatedEnvironment.Initializers.Select(x=>x.EmitterIdentity.Identity)
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
            repository.Add(new ShardAdditionRequest(hiveShardIdentity));

        var compartmentEnvironment = new CompartmentEnvironment(
            new CompartmentIdentifier(shardWorkerIsolatedEnvironment.Identifier, CompartmentType.ShardWorker), 
            serviceCollection, 
            new DependencyBuilder()
                .Add<ISimpleFabric>()
                .Add<IHiveShardTelemetry>()
                .Add<ICancellationProvider>()
                .Add<IEventRepository>()
                .Add<ISerializer>()
                .Add<GlobalChunkConfig>()
                .Build(),
            typeof(ShardWorker),
            shardWorkerIsolatedEnvironment.HiveShards.Select(x=>x.Identity)
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }
    
    private void BuildEdgeWorker(EdgeWorkerIsolatedEnvironment edgeWorkerIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<EdgeWorker>();
        serviceCollection.AddSingleton<IEdgeTunnel, EdgeTunnel>();
        var compartmentEnvironment = new CompartmentEnvironment(
            new CompartmentIdentifier(edgeWorkerIsolatedEnvironment.Identifier, CompartmentType.EdgeWorker), 
            serviceCollection, 
            new DependencyBuilder()
                .Add<CancellationProvider>()
                .Add<ICancellationProvider>()
                .Add<IAddressProvider>()
                .Add<IIdentityConfig>()
                .Add<IEdgeTunnelServerEndpoint>()
                .Build(),
            typeof(EdgeWorker),
            []
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }

    private void BuildClient(ClientIsolatedEnvironment clientIsolatedEnvironment)
    {
        ServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<ClientTunnel>();
        serviceCollection.AddSingleton<IClientTunnel, ClientTunnel>(x => x.GetRequiredService<ClientTunnel>());
        serviceCollection.AddSingleton<HiveShardClient>(clientIsolatedEnvironment.User);
        var compartmentEnvironment = new CompartmentEnvironment(
            new CompartmentIdentifier(clientIsolatedEnvironment.User.UserId, CompartmentType.Client), 
            serviceCollection, 
            new DependencyBuilder()
                .Add<IEdgeTunnelClientEndpoint>()
                .Add<ICancellationProvider>()
                .Add<IHiveShardTelemetry>()
                .Build(),
            typeof(ClientTunnel),
            []
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

        List<EmitterIdentity> emitters = new List<EmitterIdentity>();
        
        foreach (var tickerIsolatedEnvironment in tickerWorkerIsolatedEnvironment.Tickers)
        {
            tickerAdditionRepository.RequestEventTickerAddition(tickerIsolatedEnvironment.Identity);
            emitters.Add(tickerIsolatedEnvironment.Identity.ToEmitterIdentity());
        }
        foreach (var globalTickerIsolatedEnvironment in tickerWorkerIsolatedEnvironment.GlobalTickers)
        {
            tickerAdditionRepository.RequestGlobalTickerAddition(globalTickerIsolatedEnvironment.GlobalTickerIdentity);
            emitters.Add(globalTickerIsolatedEnvironment.GlobalTickerIdentity.ToEmitterType());
        }

        var compartmentIdentifier = new CompartmentIdentifier(tickerWorkerIsolatedEnvironment.TickerWorkerIdentifier, CompartmentType.TickerWorker);
        var compartmentEnvironment = new CompartmentEnvironment(
            compartmentIdentifier,
            serviceCollection,
            new DependencyBuilder()
                .Add<ICancellationProvider>()
                .Add<ISimpleFabric>()
                .Add<IEventRepository>()
                .Add<IHiveShardTelemetry>()
                .Add<ServiceEnvironment>()
                .Build(),
            typeof(TickerWorker),
            emitters
        );
        _isolatedEnvironments.Add(compartmentEnvironment);
    }
}