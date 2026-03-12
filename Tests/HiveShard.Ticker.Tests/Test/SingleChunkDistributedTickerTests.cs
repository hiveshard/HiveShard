using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Repository;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Telemetry.Console;
using HiveShard.Ticker.Data;
using HiveShard.Ticker.Tests.Data;
using HiveShard.Ticker.Tests.Events;

namespace HiveShard.Ticker.Tests.Test;

[TestFixture]
public class SingleChunkDistributedTickerTests
{
    [Test]
    public void CorrectlyRespondsToTick()
    {
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        ISerializer serializer = new NewtonsoftSerializer();
        GlobalChunkConfig globalChunkConfig = new GlobalChunkConfig(new Chunk(0, 0), new Chunk(0, 0));
        ISimpleFabric fabric = new InMemorySimpleFabric(telemetry, globalChunkConfig, serializer);
        var distributedTickerConfig = new DistributedTickerConfig(typeof(TestEvent),
            new TickerEmitterType(new EmitterIdentity("ticker")));
        EventRepository repository = new EventRepository();
        var thisEmitter = new TestEmitterType();
        repository.RegisterEvent<TestEvent>(thisEmitter);
        DistributedTicker ticker = new DistributedTicker(distributedTickerConfig, fabric, repository);
        
        ticker.Initialize();
        
        fabric.Send(typeof(Tick).FullName!, new Partition(0), new Envelope<Tick>(
            new Tick(0, [], DateTime.UtcNow, typeof(Tick).FullName!, thisEmitter.Identity),
            Guid.NewGuid()
        ));

        Tick? testEventTick = null;
        fabric.Register<Tick>(typeof(Tick).FullName!, new Partition(repository.GetEventOrder<TestEvent>()), x =>
        {
            testEventTick = x.Message.Payload;
        });
        
        Assert.That(testEventTick, Is.Not.Null);
        Assert.That(testEventTick.TickNumber, Is.EqualTo(0));
        Assert.That(testEventTick.TickEventType, Is.EqualTo(typeof(TestEvent).FullName!));
    }
    
    
    [Test]
    public void CorrectlyRespondsToCompletedTick()
    {
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        ISerializer serializer = new NewtonsoftSerializer();
        GlobalChunkConfig globalChunkConfig = new GlobalChunkConfig(new Chunk(0, 0), new Chunk(0, 0));
        ISimpleFabric fabric = new InMemorySimpleFabric(telemetry, globalChunkConfig, serializer);
        var distributedTickerEmitter = new TickerEmitterType(new EmitterIdentity("ticker"));
        var distributedTickerConfig = new DistributedTickerConfig(typeof(TestEvent), distributedTickerEmitter);
        EventRepository repository = new EventRepository();
        var thisEmitter = new TestEmitterType();
        repository.RegisterEvent<TestEvent>(thisEmitter);
        DistributedTicker ticker = new DistributedTicker(distributedTickerConfig, fabric, repository);
        
        ticker.Initialize();
        
        fabric.Send(typeof(Tick).FullName!, new Partition(0), new Envelope<Tick>(
            new Tick(0, [], DateTime.UtcNow, typeof(Tick).FullName!, thisEmitter.Identity),
            Guid.NewGuid()
        ));

        fabric.Send(typeof(CompletedTick).FullName!, new Partition(repository.GetEventOrder<TestEvent>()),
            new Envelope<CompletedTick>(
                new CompletedTick(thisEmitter.Identity, 0, typeof(TestEvent).FullName!, []),
                Guid.NewGuid()
            )
        );
        
        CompletedTick? globalTick = null;
        fabric.Register<CompletedTick>(typeof(CompletedTick).FullName!, new Partition(0), x =>
        {
            globalTick = x.Message.Payload;
        });
        
        Assert.That(globalTick, Is.Not.Null);
        Assert.That(globalTick.Tick, Is.EqualTo(0));
        Assert.That(globalTick.EventType, Is.EqualTo(typeof(TestEvent).FullName!));
        Assert.That(globalTick.EmitterIdentity, Is.EqualTo(distributedTickerEmitter.Identity));
    }
}