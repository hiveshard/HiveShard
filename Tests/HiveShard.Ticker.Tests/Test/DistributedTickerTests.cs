using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Event;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Ticker.Tests.Events;
using HiveShard.Ticker.Tests.Shards;
using HiveShard.Workers.Shard.Extensions;
using HiveShard.Workers.Ticker.Extensions;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Ticker.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
public class DistributedTickerTests<T> 
    where T: class, IDeployment, new()
{
    [Test]
    public async Task ShardCompletedTickToTicker0Tick()
    {
        string shardWorker = "SW1";
        HiveShardIdentity navigationShard = new HiveShardIdentity(
            new Chunk(0, 0),
            ShardType.From<NavigationShard>(),
            Guid.NewGuid()
        );
        IEventEmitterType testEmitter = new InitializerType("test emitter");
        var environment = HiveShardFactory.Create<T>(builder => builder
            .ShardWorker(workerBuilder => workerBuilder
                .Identify(shardWorker)
                .AddShard(navigationShard)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .Ticker<TestEvent>()
                .CentralTicker()
            )
        );

        await HiveShardTest.Given(environment, builder =>
        {
            var fabricAdapter = builder.RegisterAdapter(new HiveShardFakeFabricAdapter());
            var navigationShardAdapter = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorker, navigationShard));
            
            var initializationTopic = typeof(InitializationEvent).FullName!;
            int data = 900;
            fabricAdapter.FabricAction(fabric => fabric.Send(
                initializationTopic,
                navigationShard.Chunk,
                new InitializationEvent(data)
            ));
            fabricAdapter.FabricAction(fabric => fabric.Send("completed-ticks", 
                CompletedTick.From<InitializationEvent>(testEmitter, 0,
                    [new TopicPartitionOffset(initializationTopic, navigationShard.Chunk, 1)])));

            fabricAdapter.FabricExpectation<Tick>(x=>x.TickNumber == 1, "ticks");
            navigationShardAdapter.ExpectEvent<InitializationEvent>(x => x.Data == data);
        });
    }
}