using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Worker.Tests.Data;
using HiveShard.Worker.Tests.Events;
using HiveShard.Worker.Tests.Shards;
using HiveShard.Worker.Tests.Util;

namespace HiveShard.Worker.Tests.Test;

[TestFixture]
public class EventResponseScopedTunnelTests
{
    [Test]
    public void ShardTunnelEmitsAllProducingCompletedTicks()
    {
        ScopedTunnelTest<EchoHiveShard> test = ScopedTunnelTest<EchoHiveShard>.FromEchoShard();
        
        test.Send(new TestEvent(7));
        // wait for both input- and output-event-type ticks
        test.SendTick<TestEvent>(0, 1);
        test.SendTick<TestEventResponse>(0, 1);

        var completedTick = test.FetchCompletedTopic(new Partition(1), 0, 1)
            .Select(x => x.Message.Payload)
            .Cast<CompletedTick>()
            .FirstOrDefault();
    
        Assert.That(completedTick, Is.Not.Null);
        Assert.That(completedTick.EventType, Is.EqualTo(typeof(TestEventResponse).FullName!));
    }
}