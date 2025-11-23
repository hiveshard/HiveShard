using HiveShard.Builder;
using HiveShard.Data;
using HiveShard.Edge.events;
using HiveShard.Edge.Tests.Edge;
using HiveShard.Factory;
using HiveShard.Workers.Edge.Extensions;
using InMemory;
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
        
        var environment = HiveShardFactory.Create<InMemoryDeployment>(builder => builder
            .EdgeWorker(x => x
                .AddEdge<TestEdge>()
                .AddEdge<TestEdge2>()
                .DynamicAssignment()
            )
        );
        await HiveShardTest.RunAsync(environment, builder =>
        {
            var credentials = new HiveShardClient("test");

            var thisClient = builder.RegisterAdapter(new HiveShardClientAdapter(credentials.Username));
            

            Uri? connectedEdge = null;
            
            thisClient.Action(x => x.Connect(credentials));
            
            thisClient.Expect<ConnectionSucceeded>(x =>
            {
                connectedEdge = x.Edge;
                Console.WriteLine($"Connected to {x.Edge}");
                return true;
            });
            
            thisClient.Action(x=> x.SendHotPathEvent(new EdgeBindingRequest(credentials)));
            
            thisClient.Expect<EdgeBoundNotification>(x => x.Uri.Equals(connectedEdge));
        });
    }
    
}
