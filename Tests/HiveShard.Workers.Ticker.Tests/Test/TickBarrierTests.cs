using HiveShard.Repository;
using HiveShard.Workers.Ticker.Tests.Data;
using HiveShard.Workers.Ticker.Tests.Events;
using HiveShard.Workers.Ticker.Tests.Util;

namespace HiveShard.Workers.Ticker.Tests.Test;

[TestFixture]
public class TickBarrierTests
{
    [Test]
    public void StartsOffWithTick0()
    {
        var test = GlobalTickerTest.Empty();

        test.DeliverAll();
        var tick = test.FetchTick(0, 1);

        Assert.That(tick, Is.Not.Null);
        Assert.That(tick!.Emitter, Is.EqualTo(test.Ticker.TickerIdentity.ToEmitterType()));
    }

    [Test]
    public void RespondsToCompletedWithNextTick()
    {
        var emitter = new TestEmitterType();

        var repo = new EventRepository();
        repo.RegisterEvent<TestEvent>(emitter);

        var test = GlobalTickerTest.FromRepository(repo);

        test.SendRootCompleted<TestEvent>(emitter, 0);
        test.DeliverAll();

        var tick = test.FetchTick(1, 2);

        Assert.That(tick, Is.Not.Null);
        Assert.That(tick!.TickNumber, Is.EqualTo(1));
        Assert.That(tick.Emitter, Is.EqualTo(test.Ticker.TickerIdentity.ToEmitterType()));
    }

    [Test]
    public void TickerWaitsForInitializerOnTick0()
    {
        var initializer = new TestInitializerEmitterType();
        var emitter = new TestEmitterType();

        var repo = new EventRepository();
        repo.RegisterEvent<InitializationEvent>(initializer);
        repo.RegisterEvent<TestEvent>(emitter);

        var test = GlobalTickerTest.FromRepository(repo);

        test.SendRootCompleted<TestEvent>(emitter, 0);
        test.DeliverAll();

        Assert.That(() => test.FetchTickOrThrow(1), Throws.Exception);
    }

    [Test]
    public void TickerWaitsForAllOnTick0()
    {
        var initializer = new TestInitializerEmitterType();
        var emitter = new TestEmitterType();

        var repo = new EventRepository();
        repo.RegisterEvent<InitializationEvent>(initializer);
        repo.RegisterEvent<TestEvent>(emitter);

        var test = GlobalTickerTest.FromRepository(repo);

        test.SendRootCompleted<TestEvent>(emitter, 0);
        test.DeliverAll();

        Assert.That(() => test.FetchTickOrThrow(1), Throws.Exception);

        test.SendRootCompleted<InitializationEvent>(initializer, 0);
        test.DeliverAll();

        var tick = test.FetchTick(1, 2);
        Assert.That(tick!.TickNumber, Is.EqualTo(1));
    }

    [Test]
    public void TickerWaitsForAllOnTick1()
    {
        var initializer = new TestInitializerEmitterType();
        var emitter = new TestEmitterType();

        var repo = new EventRepository();
        repo.RegisterEvent<InitializationEvent>(initializer);
        repo.RegisterEvent<TestEvent>(emitter);

        var test = GlobalTickerTest.FromRepository(repo);

        test.SendRootCompleted<TestEvent>(emitter, 0);
        test.SendRootCompleted<InitializationEvent>(initializer, 0);
        test.DeliverAll();

        var tick1 = test.FetchTick(1, 2);
        Assert.That(tick1!.TickNumber, Is.EqualTo(1));

        test.SendRootCompleted<TestEvent>(emitter, 1);
        test.DeliverAll();

        Assert.That(() => test.FetchTickOrThrow(2), Throws.Exception);

        test.SendRootCompleted<InitializationEvent>(initializer, 1);
        test.DeliverAll();

        var tick2 = test.FetchTick(2, 3);
        Assert.That(tick2!.TickNumber, Is.EqualTo(2));
    }

    [Test]
    public void TickerDoesntWaitForInitializerAfterTick1()
    {
        var initializer = new TestInitializerEmitterType();
        var emitter = new TestEmitterType();

        var repo = new EventRepository();
        repo.RegisterEvent<InitializationEvent>(initializer);
        repo.RegisterEvent<TestEvent>(emitter);

        var test = GlobalTickerTest.FromRepository(repo);

        test.SendRootCompleted<TestEvent>(emitter, 0);
        test.SendRootCompleted<InitializationEvent>(initializer, 0);
        test.DeliverAll();

        var tick1 = test.FetchTick(1, 2);
        Assert.That(tick1!.TickNumber, Is.EqualTo(1));

        test.SendRootCompleted<TestEvent>(emitter, 1);
        test.SendRootCompleted<InitializationEvent>(initializer, 1);
        test.DeliverAll();

        var tick2 = test.FetchTick(2, 3);
        Assert.That(tick2!.TickNumber, Is.EqualTo(2));

        test.SendRootCompleted<TestEvent>(emitter, 2);
        test.DeliverAll();

        var tick3 = test.FetchTick(3, 4);
        Assert.That(tick3!.TickNumber, Is.EqualTo(3));
    }
}