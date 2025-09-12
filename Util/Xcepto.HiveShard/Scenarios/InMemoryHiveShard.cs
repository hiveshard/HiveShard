using System;
using System.Threading.Tasks;
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
using Xcepto.HiveShard.Providers;
using Xcepto.Interfaces;

namespace Xcepto.HiveShard.Scenarios
{
    public class InMemoryHiveShard: XceptoScenario
    {
        public override Task<IServiceCollection> Setup()
        {
            return Task.FromResult<IServiceCollection>(new ServiceCollection()
                .AddSingleton<ILoggingProvider, LoggingProvider>()
                .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "hiveshard"))
                .AddSingleton<IShardRepository, ShardRepository>()
                .AddSingleton<ITickRepository, TickRepository>()
                .AddSingleton<ISimpleFabric, InMemorySimpleFabric>()
                .AddSingleton<ISerializer, NewtonsoftSerializer>()
                .AddSingleton<ICancellationProvider, CancellationProvider>()
                .AddSingleton<IWorkerLoggingProvider, LoggingProvider>()
                .AddSingleton<IHiveShardSimpleLoggingProvider, LoggingProvider>()
                .AddSingleton<IFabricLoggingProvider, FabricLoggingProvider>()
                .AddSingleton<ITelemetryProvider, SimpleTelemetryProvider>()
            );
        }
    }
}