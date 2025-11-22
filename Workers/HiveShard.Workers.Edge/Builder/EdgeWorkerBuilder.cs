using HiveShard.Data;
using HiveShard.Workers.Edge.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Workers.Edge.Builder;

public class EdgeWorkerBuilder()
{
    private ServiceCollection _serviceCollection = new();
    private bool _dynamicEdgeAssignment = false;
    public EdgeWorkerBuilder AddEdge<T>()
        where T : BaseEdge
    {
        _serviceCollection.AddSingleton<T>();
        return this;
    }

    internal EdgeWorkerDefinition Build()
    {
        return new EdgeWorkerDefinition(_dynamicEdgeAssignment);
    }

    public EdgeWorkerBuilder DynamicAssignment()
    {
        _dynamicEdgeAssignment = true;
        return this;
    }
}