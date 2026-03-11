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
    public void DistributedTicker_CorrectlyRespondsToTick()
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
        
        fabric.Send("ticks", new Partition(0), new Envelope<Tick>(
            new Tick(0, [], DateTime.UtcNow, typeof(Tick).FullName!, thisEmitter.Identity),
            Guid.NewGuid()
        ));

        Tick? testEventTick = null;
        fabric.Register<Tick>("ticks", new Partition(repository.GetEventOrder<TestEvent>()), x =>
        {
            testEventTick = x.Message.Payload;
        });
        
        Assert.That(testEventTick, Is.Not.Null);
        Assert.That(testEventTick.TickNumber, Is.EqualTo(0));
        Assert.That(testEventTick.TickEventType, Is.EqualTo(typeof(TestEvent).FullName!));
    }
}