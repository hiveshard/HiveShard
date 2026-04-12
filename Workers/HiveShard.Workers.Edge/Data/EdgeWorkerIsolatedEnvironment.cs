using System;

namespace HiveShard.Workers.Edge.Data;

public class EdgeWorkerIsolatedEnvironment : IsolatedEnvironment
{
    public bool DynamicEdgeAssignment { get; }
    public Guid Identifier { get; }

    public EdgeWorkerIsolatedEnvironment(bool dynamicEdgeAssignment, Guid identifier)
    {
        Identifier = identifier;
        DynamicEdgeAssignment = dynamicEdgeAssignment;
    }

    public override bool IsUnique => false;
}