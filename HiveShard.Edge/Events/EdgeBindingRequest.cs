using HiveShard.Data;

namespace HiveShard.Edge.Events
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