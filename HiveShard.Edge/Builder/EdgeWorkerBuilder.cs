using HiveShard.Builder;
using HiveShard.Data;
using HiveShard.Edge.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Edge.Builder;

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

    internal EdgeWorkerEnvironment Build()
    {
        return new EdgeWorkerEnvironment(_dynamicEdgeAssignment);
    }

    public EdgeWorkerBuilder DynamicAssignment()
    {
        _dynamicEdgeAssignment = true;
        return this;
    }
}