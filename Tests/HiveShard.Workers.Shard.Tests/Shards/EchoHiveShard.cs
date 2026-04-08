using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Shard.Interfaces;
using HiveShard.Worker.Tests.Events;

namespace HiveShard.Worker.Tests.Shards;

public class EchoHiveShard: IHiveShard
{
    private readonly IScopedShardTunnel _scopedShardTunnel;

    public EchoHiveShard(IScopedShardTunnel scopedShardTunnel)
    {
        _scopedShardTunnel = scopedShardTunnel;
    }

    public void Initialize(Chunk chunk)
    {
        _scopedShardTunnel.Register<TestEvent>(HandleTestEvent);
    }

    private void HandleTestEvent(Message<TestEvent> obj)
    {
        _scopedShardTunnel.Send(new TestEventResponse(obj.Payload.Number));
    }
}