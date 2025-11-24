using System;
using HiveShard.Builder;
using HiveShard.Client.Builder;

namespace HiveShard.Client.Extensions;

public static class HiveShardBuilderExtensions
{
    public static DecentralizedHiveShardBuilder Client(
        this DecentralizedHiveShardBuilder serviceBuilder,
        Func<ClientBuilder, ClientBuilder> workerBuilder
    )
    {
        var builder = new ClientBuilder();
        workerBuilder(builder);
        serviceBuilder.RegisterWorker(builder.Build());
        return serviceBuilder;
    }
}