using System;
using HiveShard.Builder;
using HiveShard.Data;

namespace HiveShard.Factory;

public static class HiveShardFactory
{
    public static ServiceEnvironment Create(DeploymentType deploymentType, Action<DecentralizedHiveShardBuilder> builder)
    {
        var builderInstance = new DecentralizedHiveShardBuilder(deploymentType);
        builder(builderInstance);
        return builderInstance.Build();
    }
}