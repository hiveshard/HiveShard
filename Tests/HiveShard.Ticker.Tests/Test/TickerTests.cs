using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Ticker.Tests.Events;
using HiveShard.Ticker.Tests.Scenarios;
using Xcepto;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Ticker.Tests.Test;

[TestFixture(typeof(InMemoryScenario))]
[TestFixture(typeof(KafkaScenario))]
public class TickerTests<T> where T: XceptoScenario, new()
{
    [Test]
    public async Task TickerProducesIncrementedTick()
    {
        await XceptoTest.Given(new T(), TimeSpan.FromSeconds(20), builder =>
        {
            var ticker = builder.RegisterAdapter(new TickerXceptoAdapter(new TickerConfig(1)));
            var testFabricAccess = builder.RegisterAdapter(new SimpleFabricXceptoAdapter());

            var shard = new HiveShardIdentity(new Chunk(0, 0), new ShardType("Navigation"));

            testFabricAccess.FabricAction(x => x.Send("completed-ticks",
                new CompletedTick(shard,1,  ArraySegment<TopicPartitionOffset>.Empty, DateTime.MinValue)));

            testFabricAccess.FabricExpectation<Tick>(x => x.TickNumber == 2, "ticks");
        });
    }
}