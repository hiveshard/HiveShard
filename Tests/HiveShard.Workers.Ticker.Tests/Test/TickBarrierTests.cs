using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory.Builders;
using HiveShard.Interface.Logging;
using HiveShard.Repository;
using HiveShard.Telemetry.Console;
using HiveShard.Workers.Ticker.Data;
using HiveShard.Workers.Ticker.Tests.Data;
using HiveShard.Workers.Ticker.Tests.Events;
using HiveShard.Workers.Ticker.Tests.Extensions;

namespace HiveShard.Workers.Ticker.Tests.Test;

[TestFixture]
public class TickBarrierTests
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
        fabric.Register<Tick>(typeof(Tick).FullName!, new Partition(0), consumption =>
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
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisEmitter.Identity, 0, typeof(TestEvent).FullName!, []),
            Guid.NewGuid()
        ));
        
        Tick? lastTick = null;
        fabric.Register<Tick>(typeof(Tick).FullName!, new Partition(0), consumption =>
        {
            lastTick = consumption.Message.Payload;
        });
        Assert.That(lastTick, Is.Not.Null);
        Assert.That(lastTick.TickNumber, Is.EqualTo(1));
        Assert.That(lastTick.Emitter, Is.EqualTo(globalTickerIdentity.ToEmitterType()));
    }
    
    
    [Test]
    public void TickerWaitsForInitializerOnTick0()
    {
        var globalTickerIdentity = new GlobalTickerIdentity(Guid.NewGuid());
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        var fabric = new InMemorySimpleFabricBuilder()
            .Build(telemetry);
        var thisInitializerEmitter = new TestInitializerEmitterType();
        var thisTestEmitter = new TestEmitterType();
        var eventRepository = new EventRepository();
        eventRepository.RegisterEvent<InitializationEvent>(thisInitializerEmitter);
        eventRepository.RegisterEvent<TestEvent>(thisTestEmitter);
        
        GlobalTicker globalTicker = new GlobalTicker(globalTickerIdentity, fabric, eventRepository);
        globalTicker.Initialize();
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisTestEmitter.Identity, 0, typeof(TestEvent).FullName!, []),
            Guid.NewGuid()
        ));
        
        Assert.That(() =>
        {
            fabric.FetchTopicOffset<Tick>(new Partition(0), 1);
        }, Throws.Exception);
    }
    
    [Test]
    public void TickerWaitsForAllOnTick0()
    {
        var globalTickerIdentity = new GlobalTickerIdentity(Guid.NewGuid());
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        var fabric = new InMemorySimpleFabricBuilder()
            .Build(telemetry);
        var thisInitializerEmitter = new TestInitializerEmitterType();
        var thisTestEmitter = new TestEmitterType();
        var eventRepository = new EventRepository();
        eventRepository.RegisterEvent<InitializationEvent>(thisInitializerEmitter);
        eventRepository.RegisterEvent<TestEvent>(thisTestEmitter);
        
        GlobalTicker globalTicker = new GlobalTicker(globalTickerIdentity, fabric, eventRepository);
        globalTicker.Initialize();
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisTestEmitter.Identity, 0, typeof(TestEvent).FullName!, []),
            Guid.NewGuid()
        ));
        
        Assert.That(() =>
        {
            fabric.FetchTopicOffset<Tick>(new Partition(0), 1);
        }, Throws.Exception);
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisInitializerEmitter.Identity, 0, typeof(InitializationEvent).FullName!, []),
            Guid.NewGuid()
        ));

        var tick1 = fabric.FetchTopicOffset<Tick>(new Partition(0), 1);
        Assert.That(tick1.TickNumber, Is.EqualTo(1));
    }
    
    
    [Test]
    public void TickerWaitsForAllOnTick1()
    {
        var globalTickerIdentity = new GlobalTickerIdentity(Guid.NewGuid());
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        var fabric = new InMemorySimpleFabricBuilder()
            .Build(telemetry);
        var thisInitializerEmitter = new TestInitializerEmitterType();
        var thisTestEmitter = new TestEmitterType();
        var eventRepository = new EventRepository();
        eventRepository.RegisterEvent<InitializationEvent>(thisInitializerEmitter);
        eventRepository.RegisterEvent<TestEvent>(thisTestEmitter);
        
        GlobalTicker globalTicker = new GlobalTicker(globalTickerIdentity, fabric, eventRepository);
        globalTicker.Initialize();
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisTestEmitter.Identity, 0, typeof(TestEvent).FullName!, []),
            Guid.NewGuid()
        ));
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisInitializerEmitter.Identity, 0, typeof(InitializationEvent).FullName!, []),
            Guid.NewGuid()
        ));

        var tick1 = fabric.FetchTopicOffset<Tick>(new Partition(0), 1);
        Assert.That(tick1.TickNumber, Is.EqualTo(1));
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisTestEmitter.Identity, 1, typeof(TestEvent).FullName!, []),
            Guid.NewGuid()
        ));
        
        Assert.That(() =>
        {
            fabric.FetchTopicOffset<Tick>(new Partition(0), 2);
        }, Throws.Exception);
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisInitializerEmitter.Identity, 1, typeof(InitializationEvent).FullName!, []),
            Guid.NewGuid()
        ));

        var tick2 = fabric.FetchTopicOffset<Tick>(new Partition(0), 2);
        Assert.That(tick2.TickNumber, Is.EqualTo(2));
    }
    
    
    [Test]
    public void TickerDoesntWaitForInitializerAfterTick1()
    {
        var globalTickerIdentity = new GlobalTickerIdentity(Guid.NewGuid());
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        var fabric = new InMemorySimpleFabricBuilder()
            .Build(telemetry);
        var thisInitializerEmitter = new TestInitializerEmitterType();
        var thisTestEmitter = new TestEmitterType();
        var eventRepository = new EventRepository();
        eventRepository.RegisterEvent<InitializationEvent>(thisInitializerEmitter);
        eventRepository.RegisterEvent<TestEvent>(thisTestEmitter);
        
        GlobalTicker globalTicker = new GlobalTicker(globalTickerIdentity, fabric, eventRepository);
        globalTicker.Initialize();
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisTestEmitter.Identity, 0, typeof(TestEvent).FullName!, []),
            Guid.NewGuid()
        ));
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisInitializerEmitter.Identity, 0, typeof(InitializationEvent).FullName!, []),
            Guid.NewGuid()
        ));

        var tick1 = fabric.FetchTopicOffset<Tick>(new Partition(0), 1);
        Assert.That(tick1.TickNumber, Is.EqualTo(1));
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisTestEmitter.Identity, 1, typeof(TestEvent).FullName!, []),
            Guid.NewGuid()
        ));
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisInitializerEmitter.Identity, 1, typeof(InitializationEvent).FullName!, []),
            Guid.NewGuid()
        ));

        var tick2 = fabric.FetchTopicOffset<Tick>(new Partition(0), 2);
        Assert.That(tick2.TickNumber, Is.EqualTo(2));
        
        fabric.Send(typeof(CompletedTick).FullName!, new Partition(0), new Envelope<CompletedTick>(
            new CompletedTick(thisTestEmitter.Identity, 2, typeof(TestEvent).FullName!, []),
            Guid.NewGuid()
        ));
        
        var tick3 = fabric.FetchTopicOffset<Tick>(new Partition(0), 3);
        Assert.That(tick3.TickNumber, Is.EqualTo(3));
    }
}