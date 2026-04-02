using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Worker.Tests.Data;
using HiveShard.Worker.Tests.Events;
using HiveShard.Worker.Tests.Shards;
using HiveShard.Worker.Tests.Util;

namespace HiveShard.Worker.Tests.Test;

[TestFixture]
public class ShardStateScopedTunnelTests
{
    [Test]
    public void TunnelDelivers_AfterAllTicksReceived()
    {
        ScopedTunnelTest<TestShard> test = ScopedTunnelTest<TestShard>.FromTestShard();

        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(4));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(10));
    }

    [Test]
    public void TunnelDoesntDeliverAnything_IfTickIsMissing()
    {
        ScopedTunnelTest<TestShard> test = ScopedTunnelTest<TestShard>.FromTestShard();

        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(4));
        test.SendTick<TestEvent1>(0, 1);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(0));
    }
    
    [Test]
    public void ConsumptionDoesntSurpassTickBarrier()
    {
        ScopedTunnelTest<TestShard> test = ScopedTunnelTest<TestShard>.FromTestShard();

        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(4));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);
        test.Send(new TestEvent2(3));
        
        Assert.That(test.Shard.Sum, Is.EqualTo(10));
    }
    
    [Test]
    public void FollowUpTicksWork()
    {
        ScopedTunnelTest<TestShard> test = ScopedTunnelTest<TestShard>.FromTestShard();


        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(4));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(10));

        test.Send(new TestEvent2(3));
        test.SendTick<TestEvent1>(1, 1);
        test.SendTick<TestEvent2>(1, 2);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(13));
    }
    
    [Test]
    public void EventOrderIsFollowed()
    {
        ScopedTunnelTest<TestShard> test = ScopedTunnelTest<TestShard>.FromTestShard();


        test.Send(new TestEvent2(4, Operation.Multiplication));
        test.Send(new TestEvent1(6, Operation.Addition));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(24));
    }
    
    [Test]
    public void TunnelRespondsDoesNotRespondWithCompleted()
    {
        ScopedTunnelTest<TestShard> test = ScopedTunnelTest<TestShard>.FromTestShard();

        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(6));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);

        var completedTick = test.FetchCompletedTopic(new Partition(1), 0, 1)
            .Select(x => x.Message.Payload)
            .Cast<CompletedTick>()
            .FirstOrDefault();
        
        Assert.That(completedTick, Is.Null); // because TestShard doesn't produce anything, so it's not a dependency.
    }
}