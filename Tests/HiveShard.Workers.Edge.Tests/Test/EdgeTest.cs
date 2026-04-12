using HiveShard.Client.Extensions;
using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Workers.Edge.Events;
using HiveShard.Workers.Edge.Extensions;
using HiveShard.Workers.Edge.Tests.Edge;
using HiveShard.Workers.Edge.Tests.Events;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Workers.Edge.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
public class EdgeTest<T>
where T: IDeployment, new()
{
    [Test]
    public async Task ClientEdgeBinding()
    {
        var credentials = new HiveShardClient("test", Guid.NewGuid());
        Guid edgeWorker = Guid.NewGuid();

        var environment = HiveShardFactory.Create<T>(builder => builder
            .EdgeWorker(edgeBuilder => edgeBuilder
                .Identify(edgeWorker)
                .AddEdge<TestEdge>()
            )
            .Client(x => x
                .Identify(credentials)
            )
        );

        await HiveShardTest.Given(environment, builder =>
        {
            var edge = builder.RegisterAdapter(new HiveShardEdgeServerAdapter<TestEdge>(edgeWorker));
            var client = builder.RegisterAdapter(new HiveShardClientAdapter(credentials));
            
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
    
    [Test]
    public async Task ClientEdgeMessageTunneling()
    {
        Guid edgeWorker = Guid.NewGuid();
        var credentials = new HiveShardClient("test", Guid.NewGuid());

        var environment = HiveShardFactory.Create<T>(builder => builder
            .EdgeWorker(edgeBuilder => edgeBuilder
                .Identify(edgeWorker)
                .AddEdge<TestEdge>()
            )
            .Client(x => x
                .Identify(credentials)
            )
        );
        
        await HiveShardTest.Given(environment, builder =>
        {
            var edge = builder.RegisterAdapter(new HiveShardEdgeServerAdapter<TestEdge>(edgeWorker));
            var client = builder.RegisterAdapter(new HiveShardClientAdapter(credentials));
            Uri? connectedEdge = null;
            
            // Setup
            edge.Action(x=>x.RegisterEdgeHandler<TestEvent>((e, c) =>
            {
                x.SendEdgeEventToClient(e, c);
            }));

            client.Action(x => x.Connect(credentials));
            client.Expect<ConnectionSucceeded>(x =>
            {
                connectedEdge = x.Edge;
                Console.WriteLine($"Connected to {x.Edge}");
                return true;
            });
            client.Action(x=> x.SendHotPathEvent(new EdgeBindingRequest(credentials)));
            client.Expect<EdgeBoundNotification>(x => x.Uri.Equals(connectedEdge));
            client.Action(x=> x.SendHotPathEvent(new TestEvent()));
            client.Expect<TestEvent>(x => true);
        });
    }
}