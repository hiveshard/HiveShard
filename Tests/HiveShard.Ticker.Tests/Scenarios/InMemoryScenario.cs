using HiveShard.Config;
using HiveShard.Fabric.Ticker;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;
using Microsoft.Extensions.DependencyInjection;
using Xcepto;
using Xcepto.HiveShard.Providers;
using Xcepto.Interfaces;

namespace HiveShard.Ticker.Tests.Scenarios;

public class InMemoryScenario: XceptoScenario
{
    protected override Task<IServiceCollection> Setup()
    {
        return Task.FromResult<IServiceCollection>(new ServiceCollection()
            .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "test"))
            .AddSingleton<ITickRepository, TickRepository>()
            .AddSingleton<IHiveShardSimpleLoggingProvider, LoggingProvider>()
            .AddSingleton<ITelemetryProvider, SimpleTelemetryProvider>()
            .AddSingleton<IFabricLoggingProvider, FabricLoggingProvider>()
            .AddSingleton<IShardRepository, ShardRepository>()
            .AddSingleton<ILoggingProvider, LoggingProvider>()
            .AddSingleton<IWorkerLoggingProvider, LoggingProvider>()
            .AddSingleton<ISimpleFabric, InMemorySimpleFabric>());
    }
}