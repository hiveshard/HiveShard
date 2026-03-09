using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Serializer;
using HiveShard.Shard.Interfaces;
using HiveShard.Shard.Tests.Events;
using HiveShard.Shard.Tests.Shards;
using HiveShard.Telemetry.Console;

namespace HiveShard.Shard.Tests.Test;

[TestFixture]
public class SingleChunkScopedTunnel2Tests
{
    [Test]
    public void TunnelDelivers_AfterAllTicksReceived()
    {
        Chunk chunk = new Chunk(0,0);
        GlobalChunkConfig chunkConfig = new GlobalChunkConfig(chunk, chunk);
        ISimpleFabric fabric = CreateTunnel(chunkConfig);
        ScopedShardTunnel2 tunnel = new ScopedShardTunnel2(fabric, chunkConfig);
        TestShard shard = new TestShard(tunnel);
        var identity = new HiveShardIdentity(chunk, ShardType.From<TestShard>(), Guid.NewGuid());
        tunnel.Initialize(shard, identity);


        fabric.Send(typeof(TestEvent1).FullName!, chunk, new TestEvent1(6));
        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(4));
        fabric.Send("ticks",CreateTick<TestEvent1>(0, 1));
        fabric.Send("ticks",CreateTick<TestEvent2>(0, 1));
        
        Assert.That(shard.Sum, Is.EqualTo(10));
    }
    
    [Test]
    public void TunnelDoesntDeliverAnything_IfTickIsMissing()
    {
        Chunk chunk = new Chunk(0,0);
        GlobalChunkConfig chunkConfig = new GlobalChunkConfig(chunk, chunk);
        ISimpleFabric fabric = CreateTunnel(chunkConfig);
        ScopedShardTunnel2 tunnel = new ScopedShardTunnel2(fabric, chunkConfig);
        TestShard shard = new TestShard(tunnel);
        var identity = new HiveShardIdentity(chunk, ShardType.From<TestShard>(), Guid.NewGuid());
        tunnel.Initialize(shard, identity);


        fabric.Send(typeof(TestEvent1).FullName!, chunk, new TestEvent1(6));
        fabric.Send(typeof(TestEvent2).FullName!, chunk, new TestEvent2(4));
        fabric.Send("ticks",CreateTick<TestEvent1>(0, 1));
        
        Assert.That(shard.Sum, Is.EqualTo(0));
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