using System;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Interface;
using HiveShard.Shard;
using HiveShard.Worker;
using HiveShard.Worker.Data;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters
{
    public class WorkerXceptoAdapter: XceptoAdapter
    {
        protected override Task Initialize(IServiceProvider serviceProvider)
        {
            PropagateExceptions(serviceProvider.GetRequiredService<AllInOneWorker>().Start());
            return Task.CompletedTask;
        }

        public void AddHiveShardStep<T>()
        where T: class, IHiveShard
        {
            AddStep(new HiveShardAddingXceptoState<T>($"Adding HiveShard {typeof(T).FullName} step"));
        }
    }
}