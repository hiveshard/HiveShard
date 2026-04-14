using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Exceptions;
using HiveShard.Factory;
using HiveShard.Propagation.Tests.Config;
using HiveShard.Propagation.Tests.Data;
using HiveShard.Propagation.Tests.Events;
using HiveShard.Propagation.Tests.Initializer;
using HiveShard.Propagation.Tests.Shards;
using HiveShard.Workers.Initializer.Data;
using HiveShard.Workers.Initializer.Extensions;
using HiveShard.Workers.Shard.Extensions;
using HiveShard.Workers.Ticker.Extensions;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Propagation.Tests.Integration;

[TestFixture]
public class MultiChunkPropagationTests
{
    [Test]
    public async Task ChunkNeighbourPropagation_Works()
    {
        int secret = 7;

        Guid shardWorkerId = Guid.NewGuid();
        var firstChunk = new Chunk(0, 0);
        var secondChunk = new Chunk(1, 0);

        var propagationShardType = ShardType.From<NeighbourPropagationShardA>();
        var firstPropagationShard = new HiveShardIdentity(firstChunk, propagationShardType, Guid.NewGuid());
        var secondPropagationShard = new HiveShardIdentity(secondChunk, propagationShardType, Guid.NewGuid());

        var initializer = new InitializerEmitterIdentity(new EmitterIdentity("initializer"));

        var serviceEnvironment = HiveShardFactory.Create<InMemoryDeployment>(builder => builder
            .SetGridSize(firstChunk, secondChunk)
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<InitializingEvent>(initializer)
            )
            .Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<SingleChunkInitializer>(initializer,initializerBuilder => initializerBuilder
                    .WithDependency(new SingleChunkInitializerConfig(secret, firstChunk))
                )
            )
            .ShardWorker(shardWorker => shardWorker
                .Identify(shardWorkerId)
                .AddShard<NeighbourPropagationShardA>(firstChunk, firstPropagationShard.Id)
                .AddShard<NeighbourPropagationShardA>(secondChunk, secondPropagationShard.Id)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .GlobalTicker()
                .Ticker<InitializingEvent>()
            )
        );
        await HiveShardTest.Given(serviceEnvironment, builder =>
        {
            var firstShard = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorkerId, firstPropagationShard));
            var secondShard = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorkerId, secondPropagationShard));
            
            
            firstShard.Except<NeighbourPropagationShardA>(shard => shard.ReceivedSecret == secret);
            secondShard.Except<NeighbourPropagationShardA>(shard => shard.ReceivedSecret == secret);
        });
    }
}