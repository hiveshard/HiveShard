using HiveShard.Data;

namespace HiveShard.Edge.Data;

public class EdgeWorkerEnvironment : WorkerEnvironment
{
    private bool _dynamicEdgeAssignment;

    public EdgeWorkerEnvironment(bool dynamicEdgeAssignment)
    {
        _dynamicEdgeAssignment = dynamicEdgeAssignment;
    }
}