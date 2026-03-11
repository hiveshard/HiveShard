using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory.Builders;
using HiveShard.Interface.Logging;
using HiveShard.Repository;
using HiveShard.Telemetry.Console;
using HiveShard.Ticker.Data;
using HiveShard.Ticker.Tests.Data;
using HiveShard.Ticker.Tests.Events;

namespace HiveShard.Ticker.Tests.Test;

[TestFixture]
public class GlobalTickerTests
{
    [Test]
    public void StartsOffWithTick0()
    {
        var globalTickerIdentity = new GlobalTickerIdentity(Guid.NewGuid());
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        var fabric = new InMemorySimpleFabricBuilder()
            .Build(telemetry);
        var eventRepository = new EventRepository();
        
        GlobalTicker globalTicker = new GlobalTicker(globalTickerIdentity, fabric, eventRepository);
        globalTicker.Initialize();

        Tick? tick = null;
        fabric.Register<Tick>("ticks", new Partition(0), consumption =>
        {
            tick = consumption.Message.Payload;
        });
        Assert.That(tick, Is.Not.Null);
        Assert.That(tick.Emitter, Is.EqualTo(globalTickerIdentity.ToEmitterType()));
    }
    
    
    [Test]
    public void RespondsToCompletedWithNextTick()
    {
        var globalTickerIdentity = new GlobalTickerIdentity(Guid.NewGuid());
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        var fabric = new InMemorySimpleFabricBuilder()
            .Build(telemetry);
        var thisEmitter = new TestEmitterType();
        var eventRepository = new EventRepository();
        eventRepository.RegisterEvent<TestEvent>(thisEmitter);
        
        GlobalTicker globalTicker = new GlobalTicker(globalTickerIdentity, fabric, eventRepository);
        globalTicker.Initialize();
        
        fabric.Send("completed-ticks", new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisEmitter.Identity, 0, typeof(TestEvent).FullName!, []),
            Guid.NewGuid()
        ));
        
        Tick? lastTick = null;
        fabric.Register<Tick>("ticks", new Partition(0), consumption =>
        {
            lastTick = consumption.Message.Payload;
        });
        Assert.That(lastTick, Is.Not.Null);
        Assert.That(lastTick.TickNumber, Is.EqualTo(1));
        Assert.That(lastTick.Emitter, Is.EqualTo(globalTickerIdentity.ToEmitterType()));
    }
}