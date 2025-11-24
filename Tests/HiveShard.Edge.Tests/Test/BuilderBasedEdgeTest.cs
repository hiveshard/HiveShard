using HiveShard.Client.Extensions;
using HiveShard.Data;
using HiveShard.Edge.events;
using HiveShard.Edge.Tests.Edge;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Workers.Edge.Extensions;
using InMemory;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Edge.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
public class BuilderBasedEdgeTest<T>
where T: class, IDeployment, new()
{
    [Test]
    public async Task ClientEdgeConnection()
    {
        var clientCredentials = new HiveShardClient("test");
        
        var environment = HiveShardFactory.Create<T>(builder => builder
            .EdgeWorker(x => x
                .AddEdge<TestEdge>()
                .AddEdge<TestEdge2>()
                .DynamicAssignment()
                .Identify("EW1")
            )
            .Client(x => x
                .Identify(clientCredentials.Username)
            )
        );
        await HiveShardTest.RunAsync(environment, builder =>
        {

            var thisClient = builder.RegisterAdapter(new HiveShardClientAdapter(clientCredentials.Username));
            

            Uri? connectedEdge = null;
            
            thisClient.Action(x => x.Connect(clientCredentials));
            
            thisClient.Expect<ConnectionSucceeded>(x =>
            {
                connectedEdge = x.Edge;
                Console.WriteLine($"Connected to {x.Edge}");
                return true;
            });
            
            thisClient.Action(x=> x.SendHotPathEvent(new EdgeBindingRequest(clientCredentials)));
            
            thisClient.Expect<EdgeBoundNotification>(x => x.Uri.Equals(connectedEdge));
        });
    }
    
}
