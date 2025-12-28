using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Deployments.DockerCompose;

public class DockerComposeDeployment: IDeployment
{
    public ServiceEnvironment Build(Chunk minChunk, Chunk maxChunk, IEnumerable<IsolatedEnvironment> asEnumerable)
    {
        throw new NotImplementedException();
    }
}