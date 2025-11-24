using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Ticker.Tests.Events;
using HiveShard.Ticker.Tests.Scenarios;
using HiveShard.Workers.Ticker.Extensions;
using InMemory;
using Xcepto;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Ticker.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
public class EventTickerTests<T> 
    where T: class, IDeployment, new()
{
    [Test]
    public async Task TickerProducesIncrementedTick()
    {
        string tickerIdentifier = "TW1";
        
        var environment = HiveShardFactory.Create<T>(builder => builder
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .Identify(tickerIdentifier)
                .Ticker<TestEvent>()
            )
        );
        await HiveShardTest.Given(environment, builder =>
        {
            var ticker = builder.RegisterAdapter(new HiveShardTickerWorkerAdapter(new TickerConfig(1), tickerIdentifier));
            var testFabricAccess = builder.RegisterAdapter(new HiveShardFakeFabricAdapter());

            var shard = new HiveShardIdentity(new Chunk(0, 0), new ShardType("Navigation"));

            testFabricAccess.FabricAction(x => x.Send("completed-ticks",
                new CompletedTick(shard,1,  ArraySegment<TopicPartitionOffset>.Empty, DateTime.MinValue)));

            testFabricAccess.FabricExpectation<Tick>(x => x.TickNumber == 2, "ticks");
        });
    }
}