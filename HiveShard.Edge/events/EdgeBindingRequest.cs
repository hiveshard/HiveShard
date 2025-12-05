using HiveShard.Data;

namespace HiveShard.Edge.events
{
    public class EdgeBindingRequest
    {
        public EdgeBindingRequest(HiveShardClient hiveShardClient)
        {
            HiveShardClient = hiveShardClient;
        }

        public HiveShardClient HiveShardClient { get; }
    }
}