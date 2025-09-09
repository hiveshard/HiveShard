using System;
using System.Threading.Tasks;
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
using HiveShard.Ticker;
using Microsoft.Extensions.DependencyInjection;

namespace Hiveshard.Services.Ticker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "ticker"))
                .AddSingleton<ICancellationProvider, CancellationProvider>()
                .AddSingleton<ITelemetryProvider, TelemetryProvider>()
                .AddSingleton<ISerializer, NewtonsoftSerializer>()
                .AddSingleton<IWorkerLoggingProvider, WorkerLoggingProvider>()
                .AddSingleton<ISimpleFabric, SimpleKafkaFabric>()
                .AddSingleton<ITickRepository, TickRepository>()
                .AddSingleton<TickerConfig>(new TickerConfig(3))
                .AddSingleton<HiveShard.Ticker.Ticker>()
                .BuildServiceProvider();

            var ticker = serviceProvider.GetRequiredService<HiveShard.Ticker.Ticker>();
            var tasks = await ticker.Start();

            await Task.WhenAll(tasks);
        }
    }
}