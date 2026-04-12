using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Shard;
using HiveShard.Shard.Interfaces;
using HiveShard.Telemetry.Console;
using HiveShard.Util.Test;
using HiveShard.Worker.Tests.Data;
using HiveShard.Worker.Tests.Events;
using HiveShard.Worker.Tests.Shards;
using HiveShard.Workers.Shard;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Worker.Tests.Util;

public class ScopedTunnelTest<TShard>
    where TShard: class, IHiveShard
{
    private EventRepository _eventRepository;
    private static readonly Chunk Chunk = new(0,0);
    private static readonly TestEmitter TestEmitter = new();
    private ISimpleFabric _fabric;
    private TShard _shard;
    private HiveShardIdentity _identity;
    public TShard Shard => _shard;

    private ScopedTunnelTest(EventRepository eventRepository, HiveShardIdentity identity)
    {
        _identity = identity;
        _eventRepository = eventRepository;
        
        GlobalChunkConfig chunkConfig = new GlobalChunkConfig(Chunk, Chunk);
        _fabric = CreateTunnel(chunkConfig);
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        ScopedShardTunnel tunnel = new ScopedShardTunnel(_fabric, chunkConfig, telemetry, _eventRepository);
        
        ServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<TShard>();
        serviceCollection.AddSingleton<IScopedShardTunnel>(tunnel);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        _shard = serviceProvider.GetRequiredService<TShard>();

        tunnel.Initialize(_shard, identity);
    }

    private ISimpleFabric CreateTunnel(GlobalChunkConfig chunkConfig)
    {
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        ISerializer serializer = new NewtonsoftSerializer();
        return new InMemorySimpleFabric(telemetry, chunkConfig, serializer);
    }
    
    private Tick CreateTick<T>(long tick, long offset)
        where T: IEvent
    {
        string topic = typeof(T).FullName!;
        return new Tick(
            tick,
            [new TopicPartitionOffset(topic, new Chunk(0, 0), offset)],
            DateTime.UtcNow,
            topic,
            TestEmitter.Identity
        );
    }
    
    public void SendTick<T>(int tick, int offset)
        where T: IEvent
    {
        var partition = new Partition(_eventRepository.GetEventOrder<T>());
        _fabric.Send(typeof(Tick).FullName!, partition, _identity.Identity, CreateTick<T>(tick, offset));
    }

    public void Send<T>(T message)
        where T: IEvent => _fabric.Send(typeof(T).FullName!, Chunk, _identity.Identity, message);

    public IEnumerable<Consumption<IEnvelope<object>>> FetchCompletedTopic(Partition partition, int from, int toExclusive)
    {
        return _fabric.FetchTopic(new TopicPartition(typeof(CompletedTick).FullName!, partition), from, toExclusive);
    }

    public static ScopedTunnelTest<TestShard> FromTestShard()
    {
        HiveShardIdentity identity = new HiveShardIdentity(Chunk, ShardType.From<TestShard>(), Guid.NewGuid());
        EventRepository eventRepository = new EventRepository();
        eventRepository.RegisterEvent<TestEvent1>(TestEmitter);
        eventRepository.RegisterEvent<TestEvent2>(TestEmitter);
        return new ScopedTunnelTest<TestShard>(eventRepository, identity);
    }
    
    public static ScopedTunnelTest<EchoHiveShard> FromEchoShard()
    {
        HiveShardIdentity identity = new HiveShardIdentity(Chunk, ShardType.From<EchoHiveShard>(), Guid.NewGuid());
        EventRepository eventRepository = new EventRepository(); 
        eventRepository.RegisterEvent<TestEvent>(TestEmitter);
        eventRepository.RegisterEvent<TestEventResponse>(identity);
        return new ScopedTunnelTest<EchoHiveShard>(eventRepository, identity);
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

    public Partition GetPartition<T>()
    where T: IEvent
    {
        var eventOrder = _eventRepository.GetEventOrder<T>();
        return new Partition(eventOrder);
    }
}