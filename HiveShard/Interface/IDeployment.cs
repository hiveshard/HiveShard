using System.Collections.Generic;
using HiveShard.Data;

namespace HiveShard.Interface;

public interface IDeployment
{
    ServiceEnvironment Build(int gridSize, IEnumerable<WorkerDefinition> workers);
}