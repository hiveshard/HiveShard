using HiveShard.Builder;
using HiveShard.Data;
using HiveShard.Edge.events;
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
            var hiveShard = builder.RegisterAdapter(new HiveShardAdapter());
            var client = hiveShard.CreateClientModule();
            var edge = hiveShard.CreateEdgeModule();
            
            var credentials = new HiveShardClient("test");
            
            Uri? connectedEdge = null;
            
            client.Action(x => x.Connect(credentials));

            
            client.Expect<ConnectionSucceeded>(x =>
            {
                connectedEdge = x.Edge;
                Console.WriteLine($"Connected to {x.Edge}");
                return true;
            });
            
            client.Action(x=> x.SendHotPathEvent(new EdgeBindingRequest(credentials)));
            
            client.Expect<EdgeBoundNotification>(x => x.Uri.Equals(connectedEdge));
        });
    }
    
}
