namespace HiveShard.Workers.Edge.Data;

public class EdgeWorkerIsolatedEnvironment : IsolatedEnvironment
{
    public bool DynamicEdgeAssignment { get; }
    public string Identifier { get; }

    public EdgeWorkerIsolatedEnvironment(bool dynamicEdgeAssignment, string identifier)
    {
        Identifier = identifier;
        DynamicEdgeAssignment = dynamicEdgeAssignment;
    }

}