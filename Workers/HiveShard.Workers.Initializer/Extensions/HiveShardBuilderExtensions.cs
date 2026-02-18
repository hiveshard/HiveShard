using System;
using HiveShard.Builder;
using HiveShard.Workers.Initializer.Builder;

namespace HiveShard.Workers.Initializer.Extensions;

public static class HiveShardBuilderExtensions
{
    public static DecentralizedHiveShardBuilder Initialize(this DecentralizedHiveShardBuilder serviceBuilder,
        Func<InitializationBuilder, InitializationBuilder> initializationBuilder)
    {
        var builderInstance = new InitializationBuilder();
        initializationBuilder(builderInstance);
        serviceBuilder.RegisterWorker(builderInstance.Build());
        return serviceBuilder;
    }
}