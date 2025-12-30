using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Event;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Ticker.Tests.Events;
using HiveShard.Ticker.Tests.Shards;
using HiveShard.Workers.Shard.Extensions;
using HiveShard.Workers.Ticker.Extensions;
using Xcepto;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Ticker.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
public class EventTickerTests<T> 
    where T: class, IDeployment, new()
{
    [Test]
    public async Task TickerProducesStartupSequence()
    {
        string tickerIdentifier = "TW1";
        var onlyChunk = new Chunk(0, 0);
        var hiveShardIdentity = new HiveShardIdentity(onlyChunk, ShardType.From<NavigationShard>(), Guid.NewGuid());
        var initializerType = new InitializerType("test initializer");

        var environment = HiveShardFactory.Create<T>(builder => builder
            .SetGridSize(onlyChunk, onlyChunk)
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<TestEvent>(initializerType)
                .RegisterEvent<InitializationEvent>(initializerType)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .Identify(tickerIdentifier)
                .Ticker<TestEvent>()
            )
            .ShardWorker(shardWorkerBuilder => shardWorkerBuilder
                .AddShard<NavigationShard>(onlyChunk, hiveShardIdentity.Id)
            )
        );
        var globalChunkConfig = environment.GlobalChunkConfig;
        var eventConfig = environment.EventRepository;
        var testEventPartition = new Partition(eventConfig.GetEventOrder<TestEvent>());
        var testEventName = typeof(TestEvent).FullName!;
        var tickEventName = typeof(Tick).FullName!;
        var tickEventPartition = new Partition(0);
        var initializationEvent = new InitializationEvent(5);

        await HiveShardTest.Given(environment, builder =>
        {
            var testFabricAccess = builder.RegisterAdapter(new HiveShardFakeFabricAdapter());
            
            // Tick 0 (Hello)
            testFabricAccess.FabricExpectation<Tick>(x => 
                x.TickNumber == 0 && x.TickEventType == tickEventName, 
                "ticks", tickEventPartition);
            testFabricAccess.FabricExpectation<Tick>(x => 
                    x.TickNumber == 0 && x.TickEventType == testEventName, 
                "ticks",testEventPartition);

            // Completed 0
            testFabricAccess.FabricExpectation<CompletedTick>(x => 
                x.Tick == 0 && x.EventType == testEventName, 
                "completed-ticks", testEventPartition);
            testFabricAccess.FabricExpectation<CompletedTick>(x => 
                    x.Tick == 0 && x.EventType == tickEventName,
                "completed-ticks", tickEventPartition);
            
            // Tick 1 (Publish Initialize)
            testFabricAccess.FabricExpectation<Tick>(x => 
                    x.TickNumber == 1 && x.TickEventType == tickEventName, 
                "ticks", tickEventPartition);
            testFabricAccess.FabricExpectation<Tick>(x => 
                    x.TickNumber == 1 && x.TickEventType == testEventName, 
                "ticks", testEventPartition);
            
            testFabricAccess.FabricAction(simpleFabric => simpleFabric.Send<InitializationEvent>(
                typeof(InitializationEvent).FullName!, onlyChunk, initializationEvent));
            
            // Completed 1
            testFabricAccess.FabricExpectation<CompletedTick>(x => 
                    x.Tick == 1 && x.EventType == tickEventName, 
                "ticks", tickEventPartition);
            testFabricAccess.FabricExpectation<CompletedTick>(x => 
                    x.Tick == 1 && x.EventType == testEventName, 
                "ticks", testEventPartition);

            // Tick 2 (Shard Init Response)
            testFabricAccess.FabricExpectation<Tick>(x => 
                    x.TickNumber == 2 && x.TickEventType == tickEventName, 
                "ticks", tickEventPartition);
            testFabricAccess.FabricExpectation<Tick>(x => 
                    x.TickNumber == 2 && x.TickEventType == testEventName, 
                "ticks", testEventPartition);
            testFabricAccess.FabricExpectation<TestEvent>(x => true, 
                typeof(TestEvent).FullName!, onlyChunk.ToPartition(globalChunkConfig));
        });
    }
}