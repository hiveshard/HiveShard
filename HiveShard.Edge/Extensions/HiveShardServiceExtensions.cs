using System;
using HiveShard.Builder;
using Xcepto.HiveShard.Builder;

namespace HiveShard.Edge.Extensions;

public static class HiveShardServiceExtensions
{
    public static DecentralizedHiveShardBuilder EdgeWorker(
        this DecentralizedHiveShardBuilder builder,
        Func<EdgeWorkerBuilder, EdgeWorkerEnvironment> sourceBuilder
        )
    {
        var environment = sourceBuilder(new EdgeWorkerBuilder());
        builder.RegisterEnvironment(environment);
        return builder;
    }
}

public class EdgeSourceBuilder
{
    
}