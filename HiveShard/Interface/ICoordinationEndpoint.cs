using HiveShard.Data;

namespace HiveShard.Interface
{
    public interface ICoordinationEndpoint
    {
        EdgeConnectorDetails GetEdge(HiveShard.Data.HiveShardClient hiveShardClient);
    }
}