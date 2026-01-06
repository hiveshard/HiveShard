using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Event;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Ticker.Data;
using HiveShard.Ticker.Tests.Events;
using HiveShard.Ticker.Tests.Shards;
using HiveShard.Workers.Shard.Extensions;
using HiveShard.Workers.Ticker.Data;
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
        var initializerType = new InitializerType(new EmitterIdentity("test initializer"));

        var environment = HiveShardFactory.Create<T>(builder => builder
            .SetGridSize(onlyChunk, onlyChunk)
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<TestEvent>(hiveShardIdentity)
                .RegisterEvent<InitializationEvent>(initializerType)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .Identify(tickerIdentifier)
                .GlobalTicker(new GlobalTickerIdentity(Guid.NewGuid()))
                .Ticker<InitializationEvent>()
                .Ticker<TestEvent>()
            )
            .ShardWorker(shardWorkerBuilder => shardWorkerBuilder
                .AddShard<NavigationShard>(onlyChunk, hiveShardIdentity.Id)
            )
        );
        var globalChunkConfig = environment.GlobalChunkConfig;
        var eventConfig = environment.EventRepository;
        
        var testEventName = typeof(TestEvent).FullName!;
        var testEventPartition = new Partition(eventConfig.GetEventOrder<TestEvent>());
        
        var tickEventName = typeof(Tick).FullName!;
        var tickEventPartition = new Partition(0);

        var initializationEventName = typeof(InitializationEvent).FullName!;
        var initializationEventPartition = new Partition(eventConfig.GetEventOrder<InitializationEvent>());
        
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
            testFabricAccess.FabricExpectation<Tick>(x => 
                    x.TickNumber == 0 && x.TickEventType == initializationEventName, 
                "ticks", initializationEventPartition);
            
            // initializer hello
            testFabricAccess.FabricAction(simpleFabric => simpleFabric.Send<CompletedTick>(
                "completed-ticks", initializationEventPartition, 
                new CompletedTick(initializerType.Identity, 0, initializationEventName, 
                    [new TopicPartitionOffset(initializationEventName, onlyChunk, 0)])));


            // Completed 0
            testFabricAccess.FabricExpectation<CompletedTick>(x => 
                x.Tick == 0 && x.EventType == testEventName, 
                "completed-ticks", testEventPartition);
            testFabricAccess.FabricExpectation<CompletedTick>(x => 
                x.Tick == 0 && x.EventType == initializationEventName, 
                "completed-ticks", initializationEventPartition);
            
            // Tick 1 (Publish Initialize)
            testFabricAccess.FabricExpectation<Tick>(x => 
                    x.TickNumber == 1 && x.TickEventType == tickEventName, 
                "ticks", tickEventPartition);
            testFabricAccess.FabricExpectation<Tick>(x => 
                    x.TickNumber == 1 && x.TickEventType == testEventName, 
                "ticks", testEventPartition);
            
            testFabricAccess.FabricAction(simpleFabric => simpleFabric.Send<InitializationEvent>(
                typeof(InitializationEvent).FullName!, onlyChunk, initializationEvent));
            testFabricAccess.FabricAction(simpleFabric => simpleFabric.Send<CompletedTick>(
                "completed-ticks", initializationEventPartition, 
                new CompletedTick(initializerType.Identity, 1, initializationEventName, 
                    [new TopicPartitionOffset(initializationEventName, onlyChunk, 1)])));
            
            // Completed 1
            testFabricAccess.FabricExpectation<CompletedTick>(x => 
                    x.Tick == 1 && x.EventType == tickEventName, 
                "ticks", tickEventPartition);

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