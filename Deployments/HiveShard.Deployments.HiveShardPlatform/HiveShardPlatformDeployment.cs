using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Deployments.HiveShardPlatform;

public class HiveShardPlatformDeployment: IDeployment
{
    public ServiceEnvironment Build(int gridSize, IEnumerable<WorkerDefinition> workers)
    {
        throw new NotImplementedException();
    }
}