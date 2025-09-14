using HiveShard.Worker.Tests.Events;
using HiveShard.Worker.Tests.Scenarios;
using HiveShard.Worker.Tests.Shards;
using Xcepto;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Worker.Tests.Test;

[TestFixture(typeof(InMemoryScenario))]
[TestFixture(typeof(KafkaScenario))]
[TestFixture(typeof(HiveShardPlatformScenario))]
public class EchoShardClusterTests<T>
where T: XceptoScenario, new()
{
    private T _scenario;

    [OneTimeSetUp]
    public void SetUp()
    {
        _scenario = new T();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        
    }
    
    
    [Test]
    public async Task FirstSendTestEvent()
    {
        await XceptoTest.Given(_scenario, builder =>
        {
            var hiveShard = builder.RegisterAdapter(new HiveShardAdapter());

            hiveShard.SendEdgeMessage(new TestEvent(1));
        });
    }
    
    [Test]
    public async Task ThenExpectResponseEvent()
    {
        await XceptoTest.Given(_scenario, builder =>
        {
            var hiveShard = builder.RegisterAdapter(new HiveShardAdapter());


            hiveShard.ExpectEdgeMessage<TestEventResponse>(x => x.Number == 1);
        });
    }
}