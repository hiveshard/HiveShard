using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory.Builders;
using HiveShard.Interface;
using HiveShard.Repository;
using HiveShard.Telemetry.Console;
using HiveShard.Workers.Ticker.Data;
using HiveShard.Workers.Ticker.Tests.Data;
using HiveShard.Workers.Ticker.Tests.Events;
using HiveShard.Workers.Ticker.Tests.Util;

namespace HiveShard.Workers.Ticker.Tests.Test;

[TestFixture]
public class GlobalTickerTests
{
    [Test]
    public void GlobalTicker_StartsWithTick0()
    {
        var test = GlobalTickerTest.Empty();

        var tick = test.FetchTick(0, 1);

        Assert.That(tick, Is.Not.Null);
        Assert.That(tick!.TickNumber, Is.EqualTo(0));
    }

    [Test]
    public void GlobalTicker_DoesTick_IfAllEmittersArrived()
    {
        var emitter1 = new DistributedTickerType(1);
        var emitter2 = new DistributedTickerType(2);

        var test = GlobalTickerTest.WithEvent<CompletedTick>(emitter1, emitter2);

        test.SendRootCompleted<TestEvent>(emitter1, 0);
        test.SendRootCompleted<TestEvent>(emitter2, 0);

        var tick = test.FetchTick(1, 2);

        Assert.That(tick, Is.Not.Null);
        Assert.That(tick!.TickNumber, Is.EqualTo(1));
    }
    
    
    [Test]
    public void GlobalTicker_DoesntTick_IfNotAllEmittersArrived()
    {
        var emitter1 = new DistributedTickerType(1);
        var emitter2 = new DistributedTickerType(2);

        var test = GlobalTickerTest.WithEvent<CompletedTick>(emitter1, emitter2);

        test.SendRootCompleted<TestEvent>(emitter1, 0);

        var tick = test.FetchTick(1, 2);

        Assert.That(tick, Is.Null);
    }
}