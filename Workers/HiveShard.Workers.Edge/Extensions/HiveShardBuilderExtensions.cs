using HiveShard.Builder;
using HiveShard.Workers.Edge.Builder;

namespace HiveShard.Workers.Edge.Extensions;

public static class HiveShardBuilderExtensions
{
    public static DecentralizedHiveShardBuilder EdgeWorker(
        this DecentralizedHiveShardBuilder serviceBuilder,
        Func<EdgeWorkerBuilder, EdgeWorkerBuilder> workerBuilder
        )
    {
        var builder = new EdgeWorkerBuilder();
        workerBuilder(builder);
        serviceBuilder.RegisterWorker(builder.Build());
        return serviceBuilder;
    }
}