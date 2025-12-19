using System.Collections.Concurrent;
using HiveShard.Data;
using HiveShard.Fabrics.InMemory.Builders;
using HiveShard.Fabrics.InMemory.Tests.Data;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using HiveShard.Provider.Logging;

namespace HiveShard.Fabrics.InMemory.Tests.Test;

[TestFixture]
public class InMemorySimpleFabricTests
{
    [Test]
    public async Task RegistrationAfterPublishWorks()
    {
        var inMemorySimpleFabric = new InMemorySimpleFabricBuilder().Build();
        var id = Guid.NewGuid();
        var topic = "test";
        var chunk = new Chunk(1, 1);
        ConcurrentQueue<TestEvent> receivedMessages = new();

        await inMemorySimpleFabric.Send(topic, chunk, new TestEvent(id));
        
        inMemorySimpleFabric.Register<TestEvent>(topic, chunk, e =>
        {
            receivedMessages.Enqueue(e.Message);
        });

        var startTime = DateTime.Now;
        bool condition;
        while (true)
        {
            if (DateTime.Now - startTime > TimeSpan.FromSeconds(5))
                throw new TimeoutException("did not receive TestEvent in time");
            
            if(receivedMessages.TryDequeue(out TestEvent? testEvent))
            {
                condition = testEvent.Value.Equals(id);
                if(condition)
                    break;
            }
        }
        Assert.That(condition);
    }
}