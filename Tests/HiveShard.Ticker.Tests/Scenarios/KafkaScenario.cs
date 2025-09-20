using HiveShard.Config;
using HiveShard.Fabric.Ticker;
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

namespace HiveShard.Ticker.Tests.Scenarios;

public class KafkaScenario: XceptoScenario
{
    protected override Task<IServiceCollection> Setup()
    {
        return Task.FromResult<IServiceCollection>(new ServiceCollection()
            .AddSingleton<IShardRepository, ShardRepository>()
            .AddSingleton<IEnvironmentConfig>(new EnvironmentConfig(Guid.NewGuid()))
            .AddSingleton<IHiveShardSimpleLoggingProvider, LoggingProvider>()
            .AddSingleton<ITickRepository, TickRepository>()
            .AddSingleton<ITelemetryProvider, SimpleTelemetryProvider>()
            .AddSingleton<IFabricLoggingProvider, FabricLoggingProvider>()
            .AddSingleton<ICancellationProvider, CancellationProvider>()
            .AddSingleton<ISerializer, NewtonsoftSerializer>()
            .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "Ticker"))
            .AddSingleton<ILoggingProvider, LoggingProvider>()
            .AddSingleton<IWorkerLoggingProvider, LoggingProvider>()
            .AddSingleton<ISimpleFabric, SimpleKafkaFabric>());
    }

    private CancellationTokenSource _source = new CancellationTokenSource();
    protected override Task Initialize(IServiceProvider serviceProvider)
    {
        var fabric = serviceProvider.GetRequiredService<ISimpleFabric>();
        PropagateExceptions(fabric.Start(_source.Token));
        return Task.CompletedTask;
    }

    protected override Task Cleanup(IServiceProvider serviceProvider)
    {
        _source.Cancel();
        return Task.CompletedTask;
    }
}