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
using HiveShard.Ticker;
using Microsoft.Extensions.DependencyInjection;

namespace DotChaser.Local;

class Program
{
    static async Task Main(string[] args)
    {
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "Local DotChaser service"))
            .AddSingleton(new TickerConfig(Chunk.MaxRow))
            .AddSingleton<IShardRepository, ShardRepository>()
            .AddSingleton<ITickRepository, TickRepository>()
            .AddSingleton<ITelemetryProvider, SimpleTelemetryProvider>()
            .AddSingleton<IHiveShardSimpleLoggingProvider, LoggingProvider>()
            .AddSingleton<IWorkerLoggingProvider, WorkerLoggingProvider>()
            .AddSingleton<IFabricLoggingProvider, FabricLoggingProvider>()
            .AddSingleton<ISimpleFabric, InMemorySimpleFabric>()
            .AddSingleton<Ticker>()
            .BuildServiceProvider();

        var ticker = serviceProvider.GetRequiredService<Ticker>();
        var simpleFabric = serviceProvider.GetRequiredService<ISimpleFabric>();
        await simpleFabric.Start(tokenSource.Token);
    }
}