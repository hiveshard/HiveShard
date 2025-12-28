using System.Collections.Generic;
using HiveShard.Data;

namespace HiveShard.Interface;

public interface IDeployment
{
    ServiceEnvironment Build(Chunk minChunk, Chunk maxChunk, IEnumerable<IsolatedEnvironment> asEnumerable);
}