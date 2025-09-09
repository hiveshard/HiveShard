using HiveShard.Config;
using HiveShard.Fabric.Ticker;
using HiveShard.Fabrics.InMemory;
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

public class InMemoryScenario: XceptoScenario
{
    public override Task<IServiceCollection> Setup()
    {
        return Task.FromResult<IServiceCollection>(new ServiceCollection()
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
            .AddSingleton<ISimpleFabric, InMemorySimpleFabric>()
        );
    }
}