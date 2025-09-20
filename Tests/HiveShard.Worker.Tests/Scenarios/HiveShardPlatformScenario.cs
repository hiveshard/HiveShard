using HiveShard.Config;
using HiveShard.Fabric;
using HiveShard.Fabric.Ticker;
using HiveShard.Fabrics.InMemory;
using HiveShard.Fabrics.Kafka;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Worker.Tests.Shards;
using Microsoft.Extensions.DependencyInjection;
using Xcepto;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;
using Xcepto.HiveShard.Builder;
using Xcepto.HiveShard.Providers;
using Xcepto.Interfaces;

namespace HiveShard.Worker.Tests.Scenarios;

public class HiveShardPlatformScenario: XceptoScenario
{
    private HiveShardEnvironment _environment;
    public HiveShardPlatformScenario()
    {
        _environment = new HiveShardBuilder(1, DeploymentType.HiveShardPlatform)
            .AddShard<EchoHiveShard>()
            .Build();
    }
    protected override Task<IServiceCollection> Setup()
    {
        return Task.FromResult(_environment.GetServices());
    }

    protected override Task Initialize(IServiceProvider serviceProvider)
    {
        PropagateExceptions(_environment.Start());
        return Task.CompletedTask;
    }

    protected override Task Cleanup(IServiceProvider serviceProvider)
    {
        return Task.CompletedTask;
    }
} 