using HiveShard.Builder;
using HiveShard.Edge.Extensions;
using HiveShard.Edge.Tests.Edge;
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
        var environment = HiveShardService.From(DeploymentType.InMemory, builder => builder
            .EdgeWorker(x => x
                .AddEdge<TestEdge>()
                .AddEdge<TestEdge2>()
                .Build()
            )
            .EdgeWorker(x => x
                .DynamicAssignment()
            )
        );
        await HiveShardTest.RunAsync(environment, builder =>
        {
            builder.RegisterAdapter(new HiveShardAdapter());
        });
    }
    
}
