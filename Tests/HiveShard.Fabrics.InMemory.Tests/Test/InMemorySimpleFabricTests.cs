using System.Collections.Concurrent;
using HiveShard.Data;
using HiveShard.Fabrics.InMemory.Builders;
using HiveShard.Fabrics.InMemory.Tests.Data;
using HiveShard.Telemetry.Console;

namespace HiveShard.Fabrics.InMemory.Tests.Test;

[TestFixture]
public class InMemorySimpleFabricTests
{
    [Test]
    public async Task RegistrationAfterPublishWorks()
    {
        SimpleConsoleTelemetry simpleConsoleTelemetry = new SimpleConsoleTelemetry();
        var inMemorySimpleFabric = new InMemorySimpleFabricBuilder().Build(simpleConsoleTelemetry);
        var id = Guid.NewGuid();
        var topic = "test";
        var chunk = new Chunk(1, 1);
        ConcurrentQueue<TestEvent> receivedMessages = new();

        inMemorySimpleFabric.Send(topic, chunk, new Envelope<TestEvent>(new TestEvent(id), Guid.NewGuid()));
        
        inMemorySimpleFabric.Register<TestEvent>(topic, chunk, e =>
        {
            receivedMessages.Enqueue(e.Message.Payload);
        });

        bool condition = false;
        while (receivedMessages.TryDequeue(out TestEvent? testEvent))
        {
            condition = testEvent.Value.Equals(id);
            if(condition)
                break;
        }
        Assert.That(condition);
    }
}