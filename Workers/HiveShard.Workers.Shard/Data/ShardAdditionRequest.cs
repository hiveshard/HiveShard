using HiveShard.Data;

namespace HiveShard.Workers.Shard.Data;

public class ShardAdditionRequest
{
    public HiveShardIdentity ShardIdentity { get; }

    public ShardAdditionRequest(HiveShardIdentity hiveShardIdentity)
    {
        ShardIdentity = hiveShardIdentity;
    }
}