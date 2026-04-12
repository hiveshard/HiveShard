using HiveShard.Data;

namespace HiveShard.Workers.Edge.Events;

public class EdgeBindingRequest
{
    public EdgeBindingRequest(HiveShardClient hiveShardClient)
    {
        HiveShardClient = hiveShardClient;
    }

    public HiveShardClient HiveShardClient { get; }
}