using HiveShard.Builder;
using HiveShard.Edge.Tests.Events;
using HiveShard.Fabric.Edge;

namespace HiveShard.Edge.Tests.Edge;

public class TestEdge: BaseEdge
{
    private IEdgeTunnel _edgeTunnel;
    public TestEdge(IEdgeTunnel edgeTunnel)
    {
        _edgeTunnel = edgeTunnel;
        
        _edgeTunnel.RegisterEdgeHandler<TestEvent>((x, c) =>
        {
            _edgeTunnel.SendEdgeEventToClient(new TestEvent2(), c);
        });
    }
}

public class TestEvent2

{
}