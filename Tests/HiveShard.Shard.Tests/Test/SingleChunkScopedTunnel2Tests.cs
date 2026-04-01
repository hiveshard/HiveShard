using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Shard.Interfaces;
using HiveShard.Shard.Tests.Data;
using HiveShard.Shard.Tests.Events;
using HiveShard.Shard.Tests.Shards;
using HiveShard.Telemetry.Console;
using HiveShard.Shard.Tests.Extensions;

namespace HiveShard.Shard.Tests.Test;

[TestFixture]
public class SingleChunkScopedTunnel2Tests
{
    [Test]
    public void TunnelDelivers_AfterAllTicksReceived()
    {
        ScopedTunnelTest test = new ScopedTunnelTest();

        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(4));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(10));
    }

    [Test]
    public void TunnelDoesntDeliverAnything_IfTickIsMissing()
    {
        ScopedTunnelTest test = new ScopedTunnelTest();

        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(4));
        test.SendTick<TestEvent1>(0, 1);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(0));
    }
    
    [Test]
    public void ConsumptionDoesntSurpassTickBarrier()
    {
        ScopedTunnelTest test = new ScopedTunnelTest();

        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(4));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);
        test.Send(new TestEvent2(3));
        
        Assert.That(test.Shard.Sum, Is.EqualTo(10));
    }
    
    [Test]
    public void FollowUpTicksWork()
    {
        ScopedTunnelTest test = new ScopedTunnelTest();


        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(4));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(10));

        test.Send(new TestEvent2(3));
        test.SendTick<TestEvent1>(1, 1);
        test.SendTick<TestEvent2>(1, 2);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(13));
    }
    
    [Test]
    public void EventOrderIsFollowed()
    {
        ScopedTunnelTest test = new ScopedTunnelTest();


        test.Send(new TestEvent2(4, Operation.Multiplication));
        test.Send(new TestEvent1(6, Operation.Addition));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);
        
        Assert.That(test.Shard.Sum, Is.EqualTo(24));
    }
    
    [Test]
    public void TunnelRespondsWithCompleted()
    {
        ScopedTunnelTest test = new ScopedTunnelTest();

        test.Send(new TestEvent1(6));
        test.Send(new TestEvent2(6));
        test.SendTick<TestEvent1>(0, 1);
        test.SendTick<TestEvent2>(0, 1);

        var completedTick = test.FetchCompletedTopic(new Partition(1), 0, 1)
            .Select(x => x.Message.Payload)
            .Cast<CompletedTick>()
            .FirstOrDefault();
        
        Assert.That(completedTick, Is.Not.Null);
        Assert.That(completedTick.EventType, Is.EqualTo(typeof(TestEvent1).FullName!));
        Assert.That(completedTick.Tick, Is.EqualTo(0));
    }
    
    public class ScopedTunnelTest
    {
        private EventRepository _eventRepository = new();
        private readonly Chunk _chunk = new(0,0);
        private ISimpleFabric _fabric;
        private TestShard _shard;

        public ScopedTunnelTest()
        {
            GlobalChunkConfig chunkConfig = new GlobalChunkConfig(_chunk, _chunk);
            _fabric = CreateTunnel(chunkConfig);
            IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
            ScopedShardTunnel tunnel = new ScopedShardTunnel(_fabric, chunkConfig, telemetry, _eventRepository);
            _shard = new TestShard(tunnel);
            HiveShardIdentity identity = new HiveShardIdentity(_chunk, ShardType.From<TestShard>(), Guid.NewGuid());
            _eventRepository.RegisterEvent<TestEvent1>(identity);
            _eventRepository.RegisterEvent<TestEvent2>(identity);
            tunnel.Initialize(_shard, identity);
        }

        public TestShard Shard => _shard;

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
                new EmitterIdentity("test")
            );
        }
        
        public void SendTick<T>(int tick, int offset)
        where T: IEvent
        {
            var partition = new Partition(_eventRepository.GetEventOrder<T>());
            _fabric.Send(typeof(Tick).FullName!, partition, CreateTick<T>(tick, offset));
        }

        public void Send<T>(T message)
        where T: IEvent => _fabric.Send(typeof(T).FullName!, _chunk, message);

        public IEnumerable<Consumption<IEnvelope<object>>> FetchCompletedTopic(Partition partition, int from, int toExclusive)
        {
            return _fabric.FetchTopic(new TopicPartition(typeof(CompletedTick).FullName!, partition), from, toExclusive);
        }
    }
}