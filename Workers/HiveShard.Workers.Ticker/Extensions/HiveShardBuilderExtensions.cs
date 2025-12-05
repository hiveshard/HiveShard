using HiveShard.Builder;
using HiveShard.Workers.Ticker.Builder;
using HiveShard.Workers.Ticker.Data;

namespace HiveShard.Workers.Ticker.Extensions
{
    public static class HiveShardBuilderExtensions
    {
        public static DecentralizedHiveShardBuilder TickerWorker(
            this DecentralizedHiveShardBuilder serviceBuilder,
            Func<TickerWorkerBuilder, TickerWorkerBuilder> tickerWorkerBuilder)
        {
            var workerBuilder = new TickerWorkerBuilder();
            tickerWorkerBuilder(workerBuilder);
            serviceBuilder.RegisterWorker(workerBuilder.Build());
            return serviceBuilder;
        }
    }
}