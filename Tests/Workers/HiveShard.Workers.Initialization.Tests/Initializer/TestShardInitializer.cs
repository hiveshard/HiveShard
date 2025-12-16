using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;

namespace HiveShard.Workers.Initialization.Tests.Initializer;

public class TestShardInitializer: IInitializer
{
    public Task Initialize(IInitializationTunnel tunnel)
    {
        return Task.CompletedTask;
    }
}