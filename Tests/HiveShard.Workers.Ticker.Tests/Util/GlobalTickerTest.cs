
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory.Builders;
using HiveShard.Interface;
using HiveShard.Repository;
using HiveShard.Telemetry.Console;
using HiveShard.Workers.Ticker.Data;
using HiveShard.Workers.Ticker.Tests.Data;

namespace HiveShard.Workers.Ticker.Tests.Util;

public class GlobalTickerTest
{
    private readonly ISimpleFabric _fabric;
    private readonly EventRepository _eventRepository;
    private readonly GlobalChunkConfig _chunkConfig;
    private readonly GlobalTicker _ticker;
    private GlobalTickerIdentity _identity;

    public GlobalTicker Ticker => _ticker;

    private GlobalTickerTest(EventRepository eventRepository)
    {
        var chunk = new Chunk(0, 0);
        _chunkConfig = new GlobalChunkConfig(chunk, chunk);

        _fabric = new InMemorySimpleFabricBuilder().Build(new SimpleConsoleTelemetry());
        _eventRepository = eventRepository;

        _identity = new GlobalTickerIdentity(Guid.NewGuid());
        _ticker = new GlobalTicker(_identity, _fabric, _eventRepository);

        _ticker.Initialize();
    }

    public void SendRootCompleted<T>(IEventEmitterType emitter, long tick)
        where T : IEvent
    {
        _fabric.Send(
            typeof(CompletedTick).FullName!,
            new Partition(0),
            new Envelope<CompletedTick>(
                new CompletedTick(
                    emitter.Identity,
                    tick,
                    typeof(T).FullName!,
                    []
                ),
                Guid.NewGuid(),
                _identity.ToEmitterType()
            )
        );
    }

    public void Deliver(int amount)
    {
        _fabric.CompleteDeliveries(amount);
    }
    
    public void DeliverAll()
    {
        bool passes = true;
        while (passes)
        {
            try
            {
                Deliver(1);
            }
            catch (Exception)
            {
                passes = false;
            }
        }
    }
    public Tick? FetchTick(int from, int toExclusive)
    {
        var ticks = _fabric.FetchTopic(
                new TopicChunk(
                    typeof(Tick).FullName!,
                    new Partition(0).ToChunk(_chunkConfig)
                ),
                from,
                toExclusive
            )
            .Select(x => x.Message.Payload)
            .Cast<Tick>();

        return ticks.FirstOrDefault();
    }
    
    public Tick FetchTickOrThrow(int offset)
    {
        var tick = FetchTick(offset, offset + 1);
        if (tick == null)
            throw new Exception("No tick available");
        return tick;
    }
    
    public static GlobalTickerTest Empty()
    {
        return new GlobalTickerTest(new EventRepository());
    }

    public static GlobalTickerTest WithEvent<T>(
        params DistributedTickerType[] emitters
    )
        where T : IEvent
    {
        var repo = new EventRepository();

        foreach (var emitter in emitters)
            repo.RegisterEvent<T>(emitter);

        return new GlobalTickerTest(repo);
    }
    
    public static GlobalTickerTest FromRepository(EventRepository repo)
    {
        return new GlobalTickerTest(repo);
    }
}