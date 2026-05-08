using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Factory;
using HiveShard.Workers.Initializer.Data;
using HiveShard.Workers.Initializer.Extensions;
using HiveShard.Workers.Shard.Builder;
using HiveShard.Workers.Shard.Extensions;
using HiveShard.Workers.Ticker.Extensions;
using HiveShard.Xcepto.Tests.Event;
using HiveShard.Xcepto.Tests.Initializer;
using HiveShard.Xcepto.Tests.Shards;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Builders;
using Xcepto.HiveShard.Extensions;

namespace HiveShard.Xcepto.Tests.Test;

[TestFixture]
public class ShardAggregateStateTests
{
    [Test]
    public async Task SingleChunkAllAggregate_Succeeds()
    {
        Chunk chunk = new Chunk(0, 0);
        HiveShardIdentity onlyShard = new HiveShardIdentity(chunk, ShardType.From<StateShard>(), Guid.NewGuid());
        InitializerEmitterIdentity initializerIdentity = new InitializerEmitterIdentity(new EmitterIdentity("initializer"));

        var serviceEnvironment = HiveShardFactory.Create<InMemoryDeployment>(builder => builder
            .SetGridSize(chunk, chunk)
            .ShardWorker(workerBuilder => workerBuilder
                .AddShard(onlyShard)
            )
            .Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<StateShardAllInitializer>(initializerIdentity)
            )
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<InitializationEvent>(initializerIdentity)
                .RegisterEvent<DummyEvent>(onlyShard)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .GlobalTicker()
                .Ticker<DummyEvent>()
                .Ticker<InitializationEvent>()
            )
        );

        await HiveShardTest.Given(serviceEnvironment, builder =>
        {
            builder.ExpectShards<StateShard>(shards => shards
                .Select(x => x.Initialized)
                .All(Are.EqualTo(true))
            );
        });
    }
    
    
    [Test]
    public async Task AllAggregate_Succeeds()
    {
        Chunk minChunk = new Chunk(-1, 0);
        Chunk maxChunk = new Chunk(1, 0);
        var serviceEnvironment = HiveShardFactory.Create<InMemoryDeployment>(builder =>
        {
            builder.SetGridSize(minChunk, maxChunk);

            var shards = new List<HiveShardIdentity>();
            for (int x = minChunk.XCoord; x <= maxChunk.XCoord; x++)
            {
                for (int y = minChunk.YCoord; y <= maxChunk.YCoord; y++)
                {
                    Chunk chunk = new Chunk(x, y);
                    var shardType = ShardType.From<StateShard>();
                    HiveShardIdentity identity = new HiveShardIdentity(chunk, shardType, Guid.NewGuid());
                    shards.Add(identity);
                }
            }

            builder.ShardWorker(workerBuilder =>
            {
                foreach (var shard in shards)
                {
                    workerBuilder.AddShard(shard);
                }

                return workerBuilder;
            });

            InitializerEmitterIdentity initializerIdentity =
                new InitializerEmitterIdentity(new EmitterIdentity("initializer"));
            builder.Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<StateShardAllInitializer>(initializerIdentity)
            );

            builder.Events(eventBuilder =>
            {
                eventBuilder.RegisterEvent<InitializationEvent>(initializerIdentity);

                foreach (var shard in shards)
                {
                    eventBuilder.RegisterEvent<DummyEvent>(shard);
                }

                return eventBuilder;
            });

            builder.TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .GlobalTicker()
                .Ticker<DummyEvent>()
                .Ticker<InitializationEvent>()
            );
        });

        await HiveShardTest.Given(serviceEnvironment, builder =>
        {
            builder.ExpectShards<StateShard>(shards => shards
                .Select(x => x.Initialized)
                .All(Are.EqualTo(true))
            );
        });
    }
}