using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Repository;namespace HiveShard.Deployments.DockerCompose;

public class DockerComposeDeployment: IDeployment
{
    public ServiceEnvironment Build(Chunk minChunk, Chunk maxChunk, IEnumerable<IsolatedEnvironment> asEnumerable, IEventRepository eventRepository)
    {
        throw new NotImplementedException();
    }
}