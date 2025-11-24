using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Builder;

public class DecentralizedHiveShardBuilder
{
    private IDeployment _deployment;
    private int _gridSize = 1;
    private List<IsolatedEnvironment> _workers = new();
    private ServiceCollection _topLevelServices = new();
    internal DecentralizedHiveShardBuilder(IDeployment deployment)
    {
        _deployment = deployment;
    }

    internal IServiceCollection GetServiceCollection()
    {
        return _topLevelServices;
    }

    public DecentralizedHiveShardBuilder SetGridSize(int n)
    {
        _gridSize = n;
        return this;
    }
    internal ServiceEnvironment Build()
    {
        return _deployment.Build(_gridSize, _workers.AsEnumerable());
    }

    public void RegisterWorker(IsolatedEnvironment environment)
    {
        _workers.Add(environment);
    }
}