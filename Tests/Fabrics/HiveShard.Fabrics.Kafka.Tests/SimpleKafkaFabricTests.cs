using System.Collections.Concurrent;
using HiveShard.Config;
using HiveShard.Data;
using HiveShard.Fabrics.Kafka.Tests.Data;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using Xcepto.HiveShard.Providers;

namespace HiveShard.Fabrics.Kafka.Tests;

[TestFixture]
public class SimpleKafkaFabricTests
{
    [Test]
    [Ignore("might not terminate")]
    public async Task Test()
    {
        var cancellationProvider = new CancellationProvider();
        SimpleKafkaFabric simpleKafkaFabric = new SimpleKafkaFabric(
            new IdentityConfig(Guid.NewGuid(), "test"),
            cancellationProvider,
            new NewtonsoftSerializer(),
            new FabricLoggingProvider(new SimpleTelemetryProvider(new LoggingProvider()), new TickRepository()),
            new EnvironmentConfig(Guid.NewGuid()),
            new GlobalChunkConfig(new Chunk(0,0), new Chunk(0,0))
            );

        throw new NotImplementedException();

        BlockingCollection<TestEvent> testEvents = new BlockingCollection<TestEvent>();
        simpleKafkaFabric.Register<TestEvent>("test", e =>
        {
            testEvents.Add(e.Message);
            Console.WriteLine($"Got event with number {e.Message.TestNumber}");
        });
        _ = simpleKafkaFabric.Start(cancellationProvider.GetToken());
        
        await simpleKafkaFabric.Send("test", new TestEvent(7));

        var startTime = DateTime.Now;
        bool found = false;

        while (DateTime.Now - startTime < TimeSpan.FromSeconds(5))
        {
            var testEvent = testEvents.Take();
            if (testEvent.TestNumber == 7)
            {
                found = true;
                break;
            }
        }
        Assert.That(found, "No event with TestNumber == 7 appeared within 5 seconds");
    }
}