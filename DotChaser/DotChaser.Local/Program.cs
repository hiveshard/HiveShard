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
using HiveShard.Ticker;
using HiveShard.Worker;
using HiveShard.Worker.Data;
using Microsoft.Extensions.DependencyInjection;

namespace DotChaser.Local;

class Program
{
    static async Task Main(string[] args)
    {
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "Local DotChaser service"))
            .AddSingleton(new TickerConfig(1))
            .AddSingleton(new WorkerConfig(1))
            .AddSingleton<IShardRepository, ShardRepository>()
            .AddSingleton<ITickRepository, TickRepository>()
            .AddSingleton<ITelemetryProvider, SimpleTelemetryProvider>()
            .AddSingleton<IHiveShardSimpleLoggingProvider, LoggingProvider>()
            .AddSingleton<IWorkerLoggingProvider, WorkerLoggingProvider>()
            .AddSingleton<IFabricLoggingProvider, FabricLoggingProvider>()
            .AddSingleton<ICancellationProvider, CancellationProvider>()
            .AddSingleton<ISerializer, NewtonsoftSerializer>()
            .AddSingleton<ISimpleFabric, InMemorySimpleFabric>()
            .AddSingleton<Ticker>()
            .AddSingleton<AllInOneWorker>()
            .AddSingleton<DotChaserInitializer>()
            .BuildServiceProvider();

        var worker = serviceProvider.GetRequiredService<AllInOneWorker>();
        worker.AddHiveShard<GridShard>();
        
        var ticker = serviceProvider.GetRequiredService<Ticker>();
        var initializer = serviceProvider.GetRequiredService<DotChaserInitializer>();
        var simpleFabric = serviceProvider.GetRequiredService<ISimpleFabric>();
        await Task.WhenAll(initializer.Initialize(), simpleFabric.Start(tokenSource.Token), worker.Start());
    }
}