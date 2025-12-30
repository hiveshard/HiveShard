using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Event;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Ticker.Tests.Events;
using HiveShard.Ticker.Tests.Shards;
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
    public async Task TickerProducesIncrementedTick()
    {
        string tickerIdentifier = "TW1";
        var initializerType = new InitializerType("test initializer");

        var environment = HiveShardFactory.Create<T>(builder => builder
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<TestEvent>(initializerType)
            )
            .TickerWorker(tickerWorkerBuilder => tickerWorkerBuilder
                .Identify(tickerIdentifier)
                .Ticker<TestEvent>()
            )
        );
        await HiveShardTest.Given(environment, builder =>
        {
            var testFabricAccess = builder.RegisterAdapter(new HiveShardFakeFabricAdapter());

            testFabricAccess.FabricAction(x => x.Send("completed-ticks",
                CompletedTick.From<TestEvent>(initializerType, 1, ArraySegment<TopicPartitionOffset>.Empty)));

            testFabricAccess.FabricExpectation<Tick>(x => x.TickNumber == 2, "ticks");
        });
    }
}