using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Event;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Ticker.Data;
using HiveShard.Ticker.Tests.Events;
using HiveShard.Ticker.Tests.Shards;
using HiveShard.Workers.Shard.Extensions;
using HiveShard.Workers.Ticker.Extensions;
using HiveShard.Ticker.Tests.Extensions;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Ticker.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
public class EventTickerTests<T> 
    where T: class, IDeployment, new()
{
    [Test]
    [Ignore("system not stable enough for this yet")]
    public async Task TickerProducesStartupSequence()
    {
        Guid tickerIdentifier = Guid.NewGuid();
        var onlyChunk = new Chunk(0, 0);
        var hiveShardIdentity = new HiveShardIdentity(onlyChunk, ShardType.From<NavigationShard>(), Guid.NewGuid());
        var initializerType = new InitializerType(new EmitterIdentity("test initializer"));
        var initTickerIdentity = new DistributedTickerIdentity(Guid.NewGuid(), typeof(InitializationEvent));
        var testEventIdentity = new DistributedTickerIdentity(Guid.NewGuid(), typeof(TestEvent));
        var globalTickerIdentity = new GlobalTickerIdentity(Guid.NewGuid());

        var environment = HiveShardFactory.Create<T>(builder => builder
            .SetGridSize(onlyChunk, onlyChunk)
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<TestEvent>(hiveShardIdentity)
                .RegisterEvent<InitializationEvent>(initializerType)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .Identify(tickerIdentifier)
                .GlobalTicker(globalTickerIdentity)
                .Ticker(initTickerIdentity)
                .Ticker(testEventIdentity)
            )
            .ShardWorker(shardWorkerBuilder => shardWorkerBuilder
                .AddShard<NavigationShard>(onlyChunk, hiveShardIdentity.Id)
            )
        );
        var globalChunkConfig = environment.GlobalChunkConfig;
        var eventConfig = environment.EventRepository;
        
        var initializationEventName = typeof(InitializationEvent).FullName!;
        var initializationEventPartition = new Partition(eventConfig.GetEventOrder<InitializationEvent>());
        
        var initializationEvent = new InitializationEvent(5);

        await HiveShardTest.Given(environment, builder =>
        {
            var testFabricAccess = builder.RegisterAdapter(new HiveShardFakeFabricAdapter());
            var globalTicker = builder.RegisterAdapter(new HiveShardGlobalTickerAdapter(globalTickerIdentity.ToEmitterType()));
            var testEventTicker = builder.RegisterAdapter(new HiveShardEventTickerAdapter(testEventIdentity, eventConfig));
            var initEventTicker = builder.RegisterAdapter(new HiveShardEventTickerAdapter(initTickerIdentity, eventConfig));

            // Tick 0 (Hello)
            globalTicker.ExpectTick(0);
            testEventTicker.ExpectTick(0);
            initEventTicker.ExpectTick(0);

            // initializer hello
            testFabricAccess.FabricAction(simpleFabric => simpleFabric.Send<CompletedTick>(
                "completed-ticks", initializationEventPartition, 
                new CompletedTick(initializerType.Identity, 0, initializationEventName, 
                    [new TopicPartitionOffset(initializationEventName, onlyChunk, 0)])));


            // Completed 0
            testEventTicker.ExpectCompletedTick(0);
            initEventTicker.ExpectCompletedTick(0);
            
            // Tick 1 (Publish Initialize)
            globalTicker.ExpectTick(1);
            testEventTicker.ExpectTick(1);
            initEventTicker.ExpectTick(1);

            testFabricAccess.FabricAction(simpleFabric => simpleFabric.Send<InitializationEvent>(
                typeof(InitializationEvent).FullName!, onlyChunk, initializationEvent));
            testFabricAccess.FabricAction(simpleFabric => simpleFabric.Send<CompletedTick>(
                "completed-ticks", initializationEventPartition, 
                new CompletedTick(initializerType.Identity, 1, initializationEventName, 
                    [new TopicPartitionOffset(initializationEventName, onlyChunk, 1)])));
            
            // Completed 1
            testEventTicker.ExpectCompletedTick(1);
            initEventTicker.ExpectCompletedTick(1);

            // Tick 2 (Shard Init Response)
            globalTicker.ExpectTick(2);
            testEventTicker.ExpectTick(2);
            testFabricAccess.FabricExpectation<TestEvent>(x => true, 
                typeof(TestEvent).FullName!, onlyChunk.ToPartition(globalChunkConfig));
        });
    }
}