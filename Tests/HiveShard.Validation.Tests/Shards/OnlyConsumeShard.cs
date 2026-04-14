using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Shard.Interfaces;
using HiveShard.Validation.Tests.Events;

namespace HiveShard.Validation.Tests.Shards;

public class OnlyConsumeShard: IHiveShard
{
    private IScopedShardTunnel _scopedShardTunnel;

    public OnlyConsumeShard(IScopedShardTunnel scopedShardTunnel)
    {
        _scopedShardTunnel = scopedShardTunnel;
    }

    public void Initialize(Chunk chunk)
    {
        _scopedShardTunnel.Register<InputOnlyInitEvent>(e => {});
    }
}