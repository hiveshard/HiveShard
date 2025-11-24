using HiveShard.Interface;
using HiveShard.Workers.Shard.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Workers.Shard.Builder;

public class ShardWorkerBuilder
{
    private readonly ServiceCollection _serviceCollection = new ServiceCollection();
    
    public ShardWorkerBuilder AddShard<T>()
        where T : class, IHiveShard
    {
        _serviceCollection.AddSingleton<T>();
        return this;
    }
    
    internal ShardIsolatedEnvironment Build()
    {
        return new ShardIsolatedEnvironment();
    }
}