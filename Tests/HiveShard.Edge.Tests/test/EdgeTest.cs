using HiveShard.Edge.events;
using HiveShard.Edge.Tests.Events;
using HiveShard.Edge.Tests.scenario;
using Xcepto;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Edge.Tests.test;

[TestFixture(typeof(InMemoryScenario))]
[TestFixture(typeof(TcpScenario))]
public class EdgeTest<T>
where T: XceptoScenario, new()
{
    [Test]
    public async Task ClientEdgeBinding()
    {
        await XceptoTest.Given(new T(), builder =>
        {
            var credentials = new HiveShard.Data.Client("test");
            var edge = builder.RegisterAdapter(new EdgeServerAdapter());
            var client = builder.RegisterAdapter(new EdgeClientAdapter(credentials));
            
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
        await XceptoTest.Given(new T(), builder =>
        {
            var credentials = new HiveShard.Data.Client("test");
            var edge = builder.RegisterAdapter(new EdgeServerAdapter());
            var client = builder.RegisterAdapter(new EdgeClientAdapter(credentials));
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