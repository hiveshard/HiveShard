using System.Collections.Generic;
using HiveShard.Data;
using HiveShard.Interface.Repository;

namespace HiveShard.Interface;

public interface IDeployment
{
    ServiceEnvironment Build(Chunk minChunk, Chunk maxChunk, IEnumerable<IsolatedEnvironment> asEnumerable,
        IEventRepository eventRepository, string environmentName, ValidationMode validationMode);
}