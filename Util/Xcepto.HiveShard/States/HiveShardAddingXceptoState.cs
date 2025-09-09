using System;
using System.Threading.Tasks;
using HiveShard.Interface;
using HiveShard.Shard;
using HiveShard.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.States
{
    public class HiveShardAddingXceptoState<T>: XceptoState
    where T: class, IHiveShard
    {
        public HiveShardAddingXceptoState(string name) : base(name)
        {
        }

        public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
            => Task.FromResult(true); 

        public override Task OnEnter(IServiceProvider serviceProvider)
        {
            var worker = serviceProvider.GetRequiredService<AllInOneWorker>();
            worker.AddHiveShard<T>();
            return Task.CompletedTask;
        }
    }
}