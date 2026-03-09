using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Shard.Interfaces;
using HiveShard.Shard.Tests.Events;

namespace HiveShard.Shard.Tests.Shards;

public class TestShard: IHiveShard
{
    private readonly IScopedShardTunnel2 _tunnel;
    public int Sum { get; private set; } = 0;

    public TestShard(IScopedShardTunnel2 tunnel)
    {
        _tunnel = tunnel;
    }

    public void Initialize(Chunk chunk)
    {
        _tunnel.Register<TestEvent1>(e =>
        {
            if(e.Chunk.Equals(chunk))
                Sum += e.Payload.Number;
        });
        
        _tunnel.Register<TestEvent2>(e =>
        {
            if(e.Chunk.Equals(chunk))
                Sum += e.Payload.Number;
        });
    }
}