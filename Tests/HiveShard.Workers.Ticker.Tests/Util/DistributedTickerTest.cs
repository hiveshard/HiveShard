using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Telemetry.Console;
using HiveShard.Workers.Ticker.Data;
using HiveShard.Workers.Ticker.Tests.Events;

namespace HiveShard.Workers.Ticker.Tests.Util;

public class DistributedTickerTest
{
    private readonly ISimpleFabric _fabric;
    private readonly EventRepository _repository;
    private readonly DistributedTicker _ticker;
    private readonly DistributedTickerConfig _config;

    public DistributedTicker Ticker => _ticker;
    public EventRepository Repository => _repository;

    private DistributedTickerTest(EventRepository repository)
    {
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        ISerializer serializer = new NewtonsoftSerializer();
        GlobalChunkConfig chunkConfig = new GlobalChunkConfig(new Chunk(0, 0), new Chunk(0, 0));

        _fabric = new InMemorySimpleFabric(telemetry, chunkConfig, serializer);
        _repository = repository;

        var emitter = new TickerEmitterType(new EmitterIdentity("ticker"));
        _config = new DistributedTickerConfig(typeof(TestEvent), emitter);

        _ticker = new DistributedTicker(_config, _fabric, _repository);
        _ticker.Initialize();
    }

    public static DistributedTickerTest FromRepository(EventRepository repo)
    {
        return new DistributedTickerTest(repo);
    }

    public void SendTick(long tick, IEventEmitterType emitter)
    {
        _fabric.Send(typeof(Tick).FullName!, new Partition(0),
            new Envelope<Tick>(
                new Tick(tick, [], DateTime.UtcNow, typeof(Tick).FullName!, emitter.Identity),
                Guid.NewGuid(),
                emitter.Identity
            )
        );
    }

    public void SendCompleted<T>(IEventEmitterType emitter, long tick)
        where T : IEvent
    {
        _fabric.Send(typeof(CompletedTick).FullName!, new Partition(_repository.GetEventOrder<T>()),
            new Envelope<CompletedTick>(
                new CompletedTick(emitter.Identity, tick, typeof(T).FullName!, []),
                Guid.NewGuid(),
                emitter.Identity
            )
        );
    }
    
    public Tick? FetchTick(int from, int toExclusive)
    {
        return _fabric.FetchTopic(
                new TopicPartition(
                    typeof(Tick).FullName!,
                    new Partition(_repository.GetEventOrder<TestEvent>())
                ),
                from,
                toExclusive
            )
            .Select(x => x.Message.Payload)
            .Cast<Tick>()
            .FirstOrDefault();
    }

    public CompletedTick? FetchCompleted(int from, int toExclusive)
    {
        return _fabric.FetchTopic(
                new TopicPartition(
                    typeof(CompletedTick).FullName!,
                    new Partition(0)
                ),
                from,
                toExclusive
            )
            .Select(x => x.Message.Payload)
            .Cast<CompletedTick>()
            .FirstOrDefault();
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
}