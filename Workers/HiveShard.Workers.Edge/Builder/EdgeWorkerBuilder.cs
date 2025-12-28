using HiveShard.Edge;
using HiveShard.Workers.Edge.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Workers.Edge.Builder;

public class EdgeWorkerBuilder()
{
    private ServiceCollection _serviceCollection = new();
    private bool _dynamicEdgeAssignment = false;
    private string _identifier;

    public EdgeWorkerBuilder AddEdge<T>()
        where T : BaseEdge
    {
        _serviceCollection.AddSingleton<T>();
        return this;
    }

    public EdgeWorkerBuilder Identify(string identifier)
    {
        _identifier = identifier;
        return this;
    }

    internal EdgeWorkerIsolatedEnvironment Build()
    {
        return new EdgeWorkerIsolatedEnvironment(_dynamicEdgeAssignment, _identifier);
    }

    public EdgeWorkerBuilder DynamicAssignment()
    {
        _dynamicEdgeAssignment = true;
        return this;
    }
}