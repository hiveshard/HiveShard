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
    private void Setup(out ISimpleFabric fabric, out TestShard shard, out Chunk chunk)
    {
        chunk = new Chunk(0,0);
        GlobalChunkConfig chunkConfig = new GlobalChunkConfig(chunk, chunk);
        fabric = CreateTunnel(chunkConfig);
        EventRepository eventRepository = new EventRepository();
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        ScopedShardTunnel tunnel = new ScopedShardTunnel(fabric, chunkConfig, telemetry, eventRepository);
        shard = new TestShard(tunnel);
        HiveShardIdentity identity = new HiveShardIdentity(chunk, ShardType.From<TestShard>(), Guid.NewGuid());
        eventRepository.RegisterEvent<TestEvent1>(identity);
        eventRepository.RegisterEvent<TestEvent2>(identity);
        tunnel.Initialize(shard, identity);
    }
    
    [Test]
    public void TunnelDelivers_AfterAllTicksReceived()
    {
        Setup(out var fabric, out var shard, out var chunk);
        
        fabric.Send(typeof(TestEvent1).FullName!, chunk, new TestEvent1(6));
        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(4));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent1>(0, 1));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent2>(0, 1));
        
        Assert.That(shard.Sum, Is.EqualTo(10));
    }

    [Test]
    public void TunnelDoesntDeliverAnything_IfTickIsMissing()
    {
        Setup(out var fabric, out var shard, out var chunk);

        fabric.Send(typeof(TestEvent1).FullName!, chunk, new TestEvent1(6));
        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(4));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent1>(0, 1));
        
        Assert.That(shard.Sum, Is.EqualTo(0));
    }
    
    [Test]
    public void ConsumptionDoesntSurpassTickBarrier()
    {
        Setup(out var fabric, out var shard, out var chunk);

        fabric.Send(typeof(TestEvent1).FullName!, chunk, new TestEvent1(6));
        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(4));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent1>(0, 1));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent2>(0, 1));
        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(3));
        
        Assert.That(shard.Sum, Is.EqualTo(10));
    }
    
    [Test]
    public void FollowUpTicksWork()
    {
        Setup(out var fabric, out var shard, out var chunk);


        fabric.Send(typeof(TestEvent1).FullName!, chunk, new TestEvent1(6));
        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(4));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent1>(0, 1));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent2>(0, 1));
        
        Assert.That(shard.Sum, Is.EqualTo(10));

        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(3));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent1>(1, 1));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent2>(1, 2));
        
        Assert.That(shard.Sum, Is.EqualTo(13));
    }
    
    [Test]
    public void EventOrderIsFollowed()
    {
        Setup(out var fabric, out var shard, out var chunk);


        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(4, Operation.Multiplication));
        fabric.Send(typeof(TestEvent1).FullName!, chunk, new TestEvent1(6, Operation.Addition));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent1>(0, 1));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent2>(0, 1));
        
        Assert.That(shard.Sum, Is.EqualTo(24));
    }
    
    [Test]
    public void TunnelRespondsWithCompleted()
    {
        Setup(out var fabric, out var shard, out var chunk);

        fabric.Send(typeof(TestEvent1).FullName!, chunk, new TestEvent1(6));
        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(6));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent1>(0, 1));
        fabric.Send(typeof(Tick).FullName!,CreateTick<TestEvent2>(0, 1));

        var completedTick = fabric.FetchTopic(new TopicPartition(typeof(CompletedTick).FullName!, new Partition(1)), 0, 1)
            .Select(x => x.Message.Payload)
            .Cast<CompletedTick>()
            .FirstOrDefault();
        
        Assert.That(completedTick, Is.Not.Null);
        Assert.That(completedTick.EventType, Is.EqualTo(typeof(TestEvent1).FullName!));
        Assert.That(completedTick.Tick, Is.EqualTo(0));
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

    private ISimpleFabric CreateTunnel(GlobalChunkConfig chunkConfig)
    {
        IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
        ISerializer serializer = new NewtonsoftSerializer();
        return new InMemorySimpleFabric(telemetry, chunkConfig, serializer);
    }
}