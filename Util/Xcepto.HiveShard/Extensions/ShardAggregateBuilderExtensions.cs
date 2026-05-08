using System;
using HiveShard.Interface;
using Xcepto.Builder;
using Xcepto.HiveShard.Builders;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Extensions;

public static class ShardAggregateBuilderExtensions
{
    public static TransitionBuilder ExpectShards<THiveShard>(
        this TransitionBuilder builder, 
        Func<AggregateExpectationBuilder<THiveShard, THiveShard>, AggregateExpectation<THiveShard>> expectation
    ) where THiveShard: IHiveShard
    {
        var expectationBuilder = new AggregateExpectationBuilder<THiveShard, THiveShard>(
            x => x
        );
        var aggregateExpectation = expectation(expectationBuilder);
        var state = new AggregateShardStateExpectationState<THiveShard>(
            "Shard Aggregate expectation",
            aggregateExpectation
        );
        builder.AddStep(state);
        return builder;
    }
}