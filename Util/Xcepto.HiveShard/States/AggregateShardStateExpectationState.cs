using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Workers.Shard;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.Repositories;
using Xcepto.States;
using Xcepto.HiveShard.Builders;

namespace Xcepto.HiveShard.States;

public class AggregateShardStateExpectationState<THiveShard>: XceptoState
where THiveShard: IHiveShard
{
    private readonly AggregateExpectation<THiveShard> _expectation;

    public AggregateShardStateExpectationState(string name, AggregateExpectation<THiveShard> expectation) : base(name)
    {
        _expectation = expectation;
    }

    public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
    {
        var compartmentRepository = serviceProvider.GetRequiredService<CompartmentRepository>();

        var shardWorkers = compartmentRepository
            .GetCompartments()
            .Select(x=> x.Services.GetService<ShardWorker>())
            .Where(x=> x != null)
            .Cast<ShardWorker>()
            .ToArray();

        if (shardWorkers.Any(x => !x.Initialized))
            return Task.FromResult(false);

        var shards = shardWorkers
            .SelectMany(x => x.ManagedShards)
            .Where(x => x.GetType() == typeof(THiveShard))
            .Cast<THiveShard>();

        return Task.FromResult(_expectation.Evaluate(shards));
    }

    public override Task OnEnter(IServiceProvider serviceProvider) => Task.CompletedTask;
}