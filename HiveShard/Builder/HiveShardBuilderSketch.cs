using System;
using HiveShard.Builder;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.Builder;

public class Sketch
{
    public void InMemoryTestRequirements()
    {
        var environment = new HiveShardBuilderSketch()
            .AddEdgeWorker(builder => builder
                .AddEdge<BaseEdge>()
                .AddEdge<BaseEdge>()
                .Build()
            )
            .AddShardWorker(builder => builder
                .AddShard<TestShard>()
                .Build()
            )
            .Build();
    }

    public void InMemoryMultipleInstance()
    {
        var environment = new HiveShardBuilderSketch()
            .Build();
    }
}

public class TestShard: IHiveShard
{
    public void Process(float seconds)
    {
        throw new NotImplementedException();
    }

    public void Initialize()
    {
        throw new NotImplementedException();
    }
}

public class HiveShardBuilderSketch
{
    public HiveShardEnvironment Build()
    {
        throw new System.NotImplementedException();
    }

    public HiveShardBuilderSketch AddEdgeWorker(Func<EdgeWorkerBuilder, EdgeWorkerEnvironment> func)
    {
        throw new NotImplementedException();
    }

    public HiveShardBuilderSketch AddShardWorker(Func<ShardWorkerBuilder, ShardWorkerEnvironment> func)
    {
        throw new NotImplementedException();
    }
}


public class EdgeWorkerBuilder()
{
    private ServiceCollection _serviceCollection = new();
    public EdgeWorkerBuilder AddEdge<T>()
    where T : BaseEdge
    {
        _serviceCollection.AddSingleton<T>();
        return this;
    }

    public EdgeWorkerEnvironment Build()
    {
        return new EdgeWorkerEnvironment();
    }
}
public class EdgeWorkerEnvironment { }


public class ShardWorkerBuilder()
{
    private ServiceCollection _serviceCollection = new();
    public ShardWorkerBuilder AddShard<T>()
        where T : class, IHiveShard
    {
        _serviceCollection.AddSingleton<T>();
        return this;
    }

    public ShardWorkerEnvironment Build()
    {
        return new ShardWorkerEnvironment();
    }
}

public class ShardWorkerEnvironment { }