using HiveShard.Interface;
using HiveShard.Shard.Interfaces;
using HiveShard.Worker.Tests.Events;

namespace HiveShard.Worker.Tests.Shards;

public class EchoHiveShard: IHiveShard
{
    private IScopedShardTunnel _scopedShardTunnel;

    public EchoHiveShard(IScopedShardTunnel scopedShardTunnel)
    {
        _scopedShardTunnel = scopedShardTunnel;
    }

    public void Process(float seconds)
    {
    }

    public void Initialize()
    {
        _scopedShardTunnel.Register<TestEvent>(HandleTestEvent);
    }

    private void HandleTestEvent(TestEvent obj)
    {
        _scopedShardTunnel.Send(new TestEventResponse(obj.Number));
    }
}