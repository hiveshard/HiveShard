using HiveShard.Data;

namespace HiveShard.Workers.Edge.Events;

public class EdgeUnbindingRequest
{
    public EdgeUnbindingRequest(HiveShardClient hiveShardClient)
    {
        HiveShardClient = hiveShardClient;
    }

    public HiveShardClient HiveShardClient { get; }
}