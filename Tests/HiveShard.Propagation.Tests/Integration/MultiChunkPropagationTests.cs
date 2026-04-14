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
    public async Task DirectChunkNeighbourPropagation_Works()
    {
        int secret = 7;

        Guid shardWorkerId = Guid.NewGuid();
        var propagationShardType = ShardType.From<DirectNeighbourPropagationShardA>();
        var firstPropagationShard = new HiveShardIdentity(new Chunk(0, 0), propagationShardType, Guid.NewGuid());
        var secondPropagationShard = new HiveShardIdentity(new Chunk(1, 0), propagationShardType, Guid.NewGuid());
        var thirdPropagationShard = new HiveShardIdentity(new Chunk(2, 0), propagationShardType, Guid.NewGuid());

        var initializer = new InitializerEmitterIdentity(new EmitterIdentity("initializer"));

        var serviceEnvironment = HiveShardFactory.Create<InMemoryDeployment>(builder => builder
            .SetGridSize(firstPropagationShard.Chunk, thirdPropagationShard.Chunk)
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<InitializingEvent>(initializer)
                
                // every shard has to produce something
                .RegisterEvent<DummyEventA>(firstPropagationShard)
                .RegisterEvent<DummyEventA>(secondPropagationShard)
                .RegisterEvent<DummyEventA>(thirdPropagationShard)
            )
            .Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<SingleChunkInitializer>(initializer,initializerBuilder => initializerBuilder
                    // start propagation at first chunk, second chunk should get it, third shouldn't
                    .WithDependency(new SingleChunkInitializerConfig(secret, firstPropagationShard.Chunk))
                )
            )
            .ShardWorker(shardWorker => shardWorker
                .Identify(shardWorkerId)
                .AddShard(firstPropagationShard)
                .AddShard(secondPropagationShard)
                .AddShard(thirdPropagationShard)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .GlobalTicker()
                .Ticker<InitializingEvent>()
                .Ticker<DummyEventA>()
            )
        );
        await HiveShardTest.Given(serviceEnvironment, builder =>
        {
            var firstShard = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorkerId, firstPropagationShard));
            var secondShard = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorkerId, secondPropagationShard));
            var thirdShard = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorkerId, thirdPropagationShard));
            
            
            firstShard.Except<DirectNeighbourPropagationShardA>(shard => shard.ReceivedSecret == secret);
            secondShard.Except<DirectNeighbourPropagationShardA>(shard => shard.ReceivedSecret == secret);
            thirdShard.Except<DirectNeighbourPropagationShardA>(shard => shard.ReceivedSecret != secret);
        });
    }
    
    [Test]
    public async Task IndirectChunkNeighbourPropagation_Works()
    {
        int secret = 7;

        Guid shardWorkerId = Guid.NewGuid();
        var propagationShardType = ShardType.From<IndirectNeighbourPropagationShardA>();
        var firstPropagationShard = new HiveShardIdentity(new Chunk(0, 0), propagationShardType, Guid.NewGuid());
        var secondPropagationShard = new HiveShardIdentity(new Chunk(1, 0), propagationShardType, Guid.NewGuid());
        var thirdPropagationShard = new HiveShardIdentity(new Chunk(2, 0), propagationShardType, Guid.NewGuid());

        var initializer = new InitializerEmitterIdentity(new EmitterIdentity("initializer"));

        var serviceEnvironment = HiveShardFactory.Create<InMemoryDeployment>(builder => builder
            .SetGridSize(firstPropagationShard.Chunk, thirdPropagationShard.Chunk)
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<InitializingEvent>(initializer)
                .RegisterEvent<TransitioningEvent>(firstPropagationShard)
                .RegisterEvent<TransitioningEvent>(secondPropagationShard)
                .RegisterEvent<TransitioningEvent>(thirdPropagationShard)
            )
            .Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<SingleChunkInitializer>(initializer,initializerBuilder => initializerBuilder
                    // first gets it => second => third
                    .WithDependency(new SingleChunkInitializerConfig(secret, firstPropagationShard.Chunk))
                )
            )
            .ShardWorker(shardWorker => shardWorker
                .Identify(shardWorkerId)
                .AddShard(firstPropagationShard)
                .AddShard(secondPropagationShard)
                .AddShard(thirdPropagationShard)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .GlobalTicker()
                .Ticker<InitializingEvent>()
                .Ticker<TransitioningEvent>()
            )
        );
        await HiveShardTest.Given(serviceEnvironment, builder =>
        {
            var firstShard = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorkerId, firstPropagationShard));
            var secondShard = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorkerId, secondPropagationShard));
            var thirdShard = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorkerId, thirdPropagationShard));
            
            
            firstShard.Except<IndirectNeighbourPropagationShardA>(shard => shard.ReceivedSecret == secret);
            secondShard.Except<IndirectNeighbourPropagationShardA>(shard => shard.ReceivedSecret == secret);
            thirdShard.Except<IndirectNeighbourPropagationShardA>(shard => shard.ReceivedSecret == secret);
        });
    }
}