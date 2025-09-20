using HiveShard.Config;
using HiveShard.Fabric;
using HiveShard.Fabric.Ticker;
using HiveShard.Fabrics.InMemory;
using HiveShard.Fabrics.Kafka;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using Microsoft.Extensions.DependencyInjection;
using Xcepto;
using Xcepto.HiveShard.Providers;
using Xcepto.Interfaces;

namespace HiveShard.Worker.Tests.Scenarios;

public class KafkaScenario: XceptoScenario
{
    protected override Task<IServiceCollection> Setup()
    {
        return Task.FromResult<IServiceCollection>(new ServiceCollection()
            .AddSingleton<IEnvironmentConfig>(new EnvironmentConfig(Guid.NewGuid()))
            .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "test"))
            .AddSingleton<IHiveShardSimpleLoggingProvider, LoggingProvider>()
            .AddSingleton<ITelemetryProvider, SimpleTelemetryProvider>()
            .AddSingleton<IFabricLoggingProvider, FabricLoggingProvider>()
            .AddSingleton<IShardRepository, ShardRepository>()
            .AddSingleton<ISerializer, NewtonsoftSerializer>()
            .AddSingleton<ITickRepository, TickRepository>()
            .AddSingleton<ICancellationProvider, CancellationProvider>()
            .AddSingleton<IWorkerLoggingProvider, LoggingProvider>()
            .AddSingleton<ILoggingProvider, LoggingProvider>()
            .AddSingleton<ISimpleFabric, SimpleKafkaFabric>()
        );
    }

    private CancellationTokenSource _cancellationTokenSource = new();
    protected override Task Initialize(IServiceProvider serviceProvider)
    {
        var simpleFabric = serviceProvider.GetRequiredService<ISimpleFabric>();
        PropagateExceptions(simpleFabric.Start(_cancellationTokenSource.Token));
        return Task.CompletedTask;
    }

    protected override Task Cleanup(IServiceProvider serviceProvider)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
} 