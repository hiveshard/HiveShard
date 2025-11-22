using System;
using HiveShard.Builder;
using HiveShard.Workers.Shard.Builder;

namespace HiveShard.Workers.Shard.Extensions;

public static class HiveShardBuilderExtensions
{
    public static DecentralizedHiveShardBuilder ShardWorker(this DecentralizedHiveShardBuilder serviceBuilder,
        Func<ShardWorkerBuilder, ShardWorkerBuilder> workerBuilder)
    {
        var builderInstance = new ShardWorkerBuilder();
        workerBuilder(builderInstance);
        serviceBuilder.RegisterWorker(builderInstance.Build());
        return serviceBuilder;
    }
}