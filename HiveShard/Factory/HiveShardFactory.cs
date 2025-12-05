using Microsoft.Extensions.DependencyInjection;

using System;
using HiveShard.Builder;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Factory;

public static class HiveShardFactory
{
    public static ServiceEnvironment Create<TDeployment>(Action<DecentralizedHiveShardBuilder> builder)
    where TDeployment: IDeployment, new()
    {
        var builderInstance = new DecentralizedHiveShardBuilder(new TDeployment());
        builder(builderInstance);
        
        return builderInstance.Build();
    }
}