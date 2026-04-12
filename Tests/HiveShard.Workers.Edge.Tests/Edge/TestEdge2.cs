using HiveShard.Edge.Interfaces;
using HiveShard.Workers.Edge.Tests.Events;

namespace HiveShard.Workers.Edge.Tests.Edge;

public class TestEdge2: BaseEdge
{
    private readonly IEdgeTunnel _edgeTunnel;
    public TestEdge2(IEdgeTunnel edgeTunnel)
    {
        _edgeTunnel = edgeTunnel;
        
        _edgeTunnel.RegisterEdgeHandler<TestEvent>((x, c) =>
        {
            _edgeTunnel.SendEdgeEventToClient(new TestEvent2(), c);
        });
    }
}
