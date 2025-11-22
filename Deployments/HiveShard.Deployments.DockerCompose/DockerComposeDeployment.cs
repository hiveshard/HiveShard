using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Deployments.DockerCompose;

public class DockerComposeDeployment: IDeployment
{
    public ServiceEnvironment Build(int gridSize, IEnumerable<WorkerDefinition> workers)
    {
        throw new NotImplementedException();
    }
}