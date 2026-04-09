
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

    public GlobalTicker Ticker => _ticker;

    private GlobalTickerTest(EventRepository eventRepository)
    {
        var chunk = new Chunk(0, 0);
        _chunkConfig = new GlobalChunkConfig(chunk, chunk);

        _fabric = new InMemorySimpleFabricBuilder().Build(new SimpleConsoleTelemetry());
        _eventRepository = eventRepository;

        var identity = new GlobalTickerIdentity(Guid.NewGuid());
        _ticker = new GlobalTicker(identity, _fabric, _eventRepository);

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
                Guid.NewGuid()
            )
        );
    }

    public Tick? FetchTick(int from, int toExclusive)
    {
        return _fabric.FetchTopic(
                new TopicChunk(
                    typeof(Tick).FullName!,
                    new Partition(0).ToChunk(_chunkConfig)
                ),
                from,
                toExclusive
            )
            .Select(x => x.Message.Payload)
            .Cast<Tick>()
            .FirstOrDefault();
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
}