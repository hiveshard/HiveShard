using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Validation.Tests.Shards;

public class NoEventShard: IHiveShard
{
    public void Initialize(Chunk chunk) { }
}