using HiveShard.Repository;
using HiveShard.Workers.Ticker.Tests.Data;
using HiveShard.Workers.Ticker.Tests.Events;
using HiveShard.Workers.Ticker.Tests.Util;

namespace HiveShard.Workers.Ticker.Tests.Test;

[TestFixture]
public class SingleChunkDistributedTickerTests
{
    [Test]
    public void CorrectlyRespondsToTick()
    {
        var emitter = new TestEmitterType();

        var repo = new EventRepository();
        repo.RegisterEvent<TestEvent>(emitter);

        var test = DistributedTickerTest.FromRepository(repo);

        test.SendTick(0, emitter);
        test.DeliverAll();

        var tick = test.FetchTick(0, 1);

        Assert.That(tick, Is.Not.Null);
        Assert.That(tick!.TickNumber, Is.EqualTo(0));
        Assert.That(tick.TickEventType, Is.EqualTo(typeof(TestEvent).FullName!));
    }

    [Test]
    public void CorrectlyRespondsToCompletedTick()
    {
        var emitter = new TestEmitterType();

        var repo = new EventRepository();
        repo.RegisterEvent<TestEvent>(emitter);

        var test = DistributedTickerTest.FromRepository(repo);

        test.SendTick(0, emitter);
        test.SendCompleted<TestEvent>(emitter, 0);
        test.DeliverAll();

        var completed = test.FetchCompleted(0, 1);

        Assert.That(completed, Is.Not.Null);
        Assert.That(completed!.Tick, Is.EqualTo(0));
        Assert.That(completed.EventType, Is.EqualTo(typeof(TestEvent).FullName!));
        Assert.That(completed.EmitterIdentity, Is.EqualTo(test.Ticker.Identity));
    }
}