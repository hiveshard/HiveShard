using System;
using System.Collections.Generic;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Builder;

namespace HiveShard.Builder;

public static class HiveShardService
{
    public static HiveShardEnvironment From(DeploymentType deploymentType, Action<DecentralizedHiveShardBuilder> builder)
    {
        var builderInstance = new DecentralizedHiveShardBuilder(deploymentType);
        builder(builderInstance);
        return builderInstance.Build();
    }
}

public abstract class HiveShardServiceSources
{
    private DeploymentType _deploymentType;
    private DecentralizedHiveShardBuilder _builder;

    internal void Initialize(DeploymentType deploymentType, DecentralizedHiveShardBuilder builder)
    {
        _builder = builder;
        _deploymentType = deploymentType;
    }
}

public class DecentralizedHiveShardBuilder
{
    private DeploymentType _deploymentType;
    private List<HiveShardServiceSources> _adapters = new();
    public DecentralizedHiveShardBuilder(DeploymentType deploymentType)
    {
        _deploymentType = deploymentType;
    }

    internal HiveShardEnvironment Build()
    {
        throw new NotImplementedException();
    }

    public TAdapter RegisterSources<TAdapter>()
    where TAdapter: HiveShardServiceSources, new()
    {
        var adapter = new TAdapter();
        _adapters.Add(adapter);
        adapter.Initialize(_deploymentType, this);
        return adapter;
    }

    public void RegisterEnvironment(EdgeWorkerEnvironment environment)
    {
        throw new NotImplementedException();
    }
}
public class SpecialEdge1: BaseEdge { }
public class SpecialEdge2: BaseEdge { }
public abstract class BaseEdge { }