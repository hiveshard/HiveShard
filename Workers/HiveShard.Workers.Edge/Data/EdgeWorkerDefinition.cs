namespace HiveShard.Workers.Edge.Data;

public class EdgeWorkerDefinition : WorkerDefinition
{
    private bool _dynamicEdgeAssignment;

    public EdgeWorkerDefinition(bool dynamicEdgeAssignment)
    {
        _dynamicEdgeAssignment = dynamicEdgeAssignment;
    }
}