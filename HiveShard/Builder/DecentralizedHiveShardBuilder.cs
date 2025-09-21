using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Builder;

public class DecentralizedHiveShardBuilder
{
    private DeploymentType _deploymentType;
    private int _gridSize = 1;
    private List<WorkerEnvironment> _workers = new();
    private ServiceCollection _topLevelServices = new();
    public DecentralizedHiveShardBuilder(DeploymentType deploymentType)
    {
        _deploymentType = deploymentType;
    }

    public void SetGridSize(int n)
    {
        _gridSize = n;
    }
    internal ServiceEnvironment Build()
    {
        return new ServiceEnvironment(_gridSize, _deploymentType, _topLevelServices, _workers.AsEnumerable());
    }

    public void RegisterEnvironment(WorkerEnvironment environment)
    {
        _workers.Add(environment);
    }
}