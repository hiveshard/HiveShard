using HiveShard.Interface;
using HiveShard.Shard.Interfaces;
using HiveShard.Shard.Tests.Events;

namespace HiveShard.Shard.Tests.Shards;

public class TestShard: IHiveShard
{
    private readonly IScopedShardTunnel _tunnel;
    private TestEvent _testEvent;

    public TestShard(IScopedShardTunnel tunnel)
    {
        _tunnel = tunnel;
    }

    public void Initialize()
    {
        _tunnel.Register<TestEvent>(e =>
        {
            _testEvent = e;
        });
    }
}