using HiveShard.Builder;
using HiveShard.Data;
using HiveShard.Edge.Extensions;
using HiveShard.Edge.Tests.Edge;
using HiveShard.Factory;
using Xcepto;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Edge.Tests.test;

[TestFixture]
public class BuilderBasedEdgeTest
{
    [Test]
    public async Task TestEdgeBuilderBased()
    {
        var environment = HiveShardFactory.Create(DeploymentType.InMemory, builder => builder
            .EdgeWorker(x => x
                .AddEdge<TestEdge>()
                .AddEdge<TestEdge2>()
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
