using HiveShard.Data;

namespace HiveShard.Edge.events
{
    public class EdgeUnbindingRequest
    {
        public EdgeUnbindingRequest(HiveShardClient hiveShardClient)
        {
            HiveShardClient = hiveShardClient;
        }

        public HiveShardClient HiveShardClient { get; }
    }
}