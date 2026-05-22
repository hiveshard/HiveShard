using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Factory;
using HiveShard.Initializer.Interfaces;
using HiveShard.Workers.Initializer.Data;
using HiveShard.Workers.Initializer.Extensions;
using HiveShard.Workers.Shard.Builder;
using HiveShard.Workers.Shard.Extensions;
using HiveShard.Workers.Ticker.Extensions;
using HiveShard.Xcepto.Tests.Event;
using HiveShard.Xcepto.Tests.Initializer;
using HiveShard.Xcepto.Tests.Shards;
using Xcepto.Config;
using Xcepto.Exceptions;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Builders;
using Xcepto.HiveShard.Extensions;

namespace HiveShard.Xcepto.Tests.Test;

[TestFixture]
public class ShardAggregateStateTests
{
    private ServiceEnvironment GetSingleChunkEnvironment<TInitializer>()
    where TInitializer : IInitializer
    {
        Chunk chunk = new Chunk(0, 0);
        HiveShardIdentity onlyShard = new HiveShardIdentity(chunk, ShardType.From<StateShard>(), Guid.NewGuid());
        InitializerEmitterIdentity initializer = new InitializerEmitterIdentity(
            new EmitterIdentity(typeof(TInitializer).FullName!)
        );

        return HiveShardFactory.Create<InMemoryDeployment>(builder => builder
            .SetGridSize(chunk, chunk)
            .ShardWorker(workerBuilder => workerBuilder
                .AddShard(onlyShard)
            )
            .Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<TInitializer>(initializer, dependencies => dependencies
                    .WithDependency(chunk)
                )
            )
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<InitializationEvent>(initializer)
                .RegisterEvent<DummyEvent>(onlyShard)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .GlobalTicker()
                .Ticker<DummyEvent>()
                .Ticker<InitializationEvent>()
            )
        );
    }

    private ServiceEnvironment GetMultiChunkEnvironment<TInitializer>()
    where TInitializer : IInitializer
    {
        Chunk minChunk = new Chunk(-1, 0);
        Chunk maxChunk = new Chunk(1, 0);
        return HiveShardFactory.Create<InMemoryDeployment>(builder =>
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

            InitializerEmitterIdentity initializerIdentity = new InitializerEmitterIdentity(new EmitterIdentity(typeof(TInitializer).FullName!));
            builder.Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<TInitializer>(initializerIdentity, dependencies => dependencies
                    .WithDependency(minChunk)
                )
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
    }
    
    [Test]
    public async Task SingleChunkAllAggregate_Succeeds()
    {
        var environment = GetSingleChunkEnvironment<AllTargetShardInitializer>();
        await HiveShardTest.Given(environment, builder =>
        {
            builder.ExpectShards<StateShard>(shards => shards
                .Select(x => x.Initialized)
                .All(Are.EqualTo(true))
            );
        });
    }
    
    [Test]
    public async Task SingleChunkAnyAggregate_Succeeds()
    {
        var environment = GetSingleChunkEnvironment<SpecificTargetShardInitializer>();
        await HiveShardTest.Given(environment, builder =>
        {
            builder.ExpectShards<StateShard>(shards => shards
                .Select(x => x.Initialized)
                .Any(Are.EqualTo(true))
            );
        });
    }
    
    
    [Test]
    public async Task MultiChunkAllAggregate_Succeeds()
    {
        var environment = GetMultiChunkEnvironment<AllTargetShardInitializer>();
        await HiveShardTest.Given(environment, builder =>
        {
            builder.ExpectShards<StateShard>(shards => shards
                .Select(x => x.Initialized)
                .All(Are.EqualTo(true))
            );
        });
    }
    
    [Test]
    public void MultiChunkAllAggregate_Fails()
    {
        Assert.ThatAsync(async () =>
        {
            var environment = GetMultiChunkEnvironment<SpecificTargetShardInitializer>();
            await HiveShardTest.Given(environment, builder =>
            {
                builder.ExpectShards<StateShard>(shards => shards
                    .Select(x => x.Initialized)
                    .All(Are.EqualTo(true))
                );
            });
        }, Throws.Exception.Message.Contains("Expected: All are equal to True")
            .And.Message.Contains("But was: [true,false,false]")
        );
    }
    
    [Test]
    public async Task MultiChunkAnyAggregate_Succeeds()
    {
        var environment = GetMultiChunkEnvironment<SpecificTargetShardInitializer>();
        await HiveShardTest.Given(environment, builder =>
        {
            builder.ExpectShards<StateShard>(shards => shards
                .Select(x => x.Initialized)
                .Any(Are.EqualTo(true))
            );
        }); 
    }
    
    
    [Test]
    public void MultiChunkAnyAggregate_Fails()
    {
        Assert.ThatAsync(async () =>
            {
                var environment = GetMultiChunkEnvironment<NoTargetShardInitializer>();
                await HiveShardTest.Given(environment, builder =>
                {
                    builder.ExpectShards<StateShard>(shards => shards
                        .Select(x => x.Initialized)
                        .Any(Are.EqualTo(true))
                    );
                });
            }, Throws.Exception.Message.Contains("Expected: Any are equal to True")
                .And.Message.Contains("But was: [false,false,false]")
        );
    }
}