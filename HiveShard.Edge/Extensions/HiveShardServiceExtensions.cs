using System;
using HiveShard.Builder;
using HiveShard.Edge.Builder;
using HiveShard.Edge.Data;

namespace HiveShard.Edge.Extensions;

public static class HiveShardServiceExtensions
{
    public static DecentralizedHiveShardBuilder EdgeWorker(
        this DecentralizedHiveShardBuilder serviceBuilder,
        Func<EdgeWorkerBuilder, EdgeWorkerBuilder> workerBuilder
        )
    {
        var builder = new EdgeWorkerBuilder();
        workerBuilder(builder);
        serviceBuilder.RegisterEnvironment(builder.Build());
        return serviceBuilder;
    }
}