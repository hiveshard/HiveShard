using HiveShard.Builder;
using HiveShard.Edge.Adapters;
using Xcepto;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;
using Xcepto.HiveShard.Builder;
using HiveShardEnvironment = Xcepto.HiveShard.HiveShardEnvironment;

namespace HiveShard.Edge.Tests.test;

[TestFixture]
public class BuilderBasedEdgeTest
{
    [Test]
    public async Task TestEdgeBuilderBased()
    {
        var environment = HiveShardService.From(DeploymentType.InMemory, builder =>
        {
            builder.RegisterAdapter<EdgeAdapter>();
        });
        await HiveShardTest.RunAsync(environment, builder =>
        {
            builder.RegisterAdapter(new HiveShardAdapter());
        });
    }
    
}
