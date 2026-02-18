using HiveShard.Data;

namespace HiveShard.Edge.Events;

public class EdgeUnbindingRequest
{
    public EdgeUnbindingRequest(HiveShardClient hiveShardClient)
    {
        HiveShardClient = hiveShardClient;
    }

    public HiveShardClient HiveShardClient { get; }
}