using HiveShard.Edge.events;
using HiveShard.Edge.Tests.Edge;
using HiveShard.Edge.Tests.Events;
using HiveShard.Edge.Tests.scenario;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Workers.Edge.Extensions;
using InMemory;
using Xcepto;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Edge.Tests.test;

[TestFixture(typeof(InMemoryDeployment))]
public class EdgeTest<T>
where T: IDeployment, new()
{
    [Test]
    public async Task ClientEdgeBinding()
    {
        var environment = HiveShardFactory.Create<T>(builder => builder
            .EdgeWorker(edgeBuilder => edgeBuilder
                .AddEdge<TestEdge>()
            )
        );

        await HiveShardTest.RunAsync(environment, builder =>
        {
            var credentials = new HiveShard.Data.HiveShardClient("test");
            var edge = builder.RegisterAdapter(new HiveShardEdgeServerAdapter<TestEdge>());
            var client = builder.RegisterAdapter(new HiveShardClientAdapter(credentials.Username));
            
            Uri? connectedEdge = null;

            edge.Action(x=>x.Start());
            
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
        var environment = HiveShardFactory.Create<T>(builder => builder
            .EdgeWorker(edgeBuilder => edgeBuilder
                .AddEdge<TestEdge>()
            )
        );
        
        await HiveShardTest.RunAsync(environment, builder =>
        {
            var credentials = new HiveShard.Data.HiveShardClient("test");
            var edge = builder.RegisterAdapter(new HiveShardEdgeServerAdapter<TestEdge>());
            var client = builder.RegisterAdapter(new HiveShardClientAdapter(credentials.Username));
            Uri? connectedEdge = null;
            
            // Setup
            edge.Action(x=>x.RegisterEdgeHandler<TestEvent>((e, c) =>
            {
                x.SendEdgeEventToClient(e, c);
            }));

            edge.Action(x=>x.Start());

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