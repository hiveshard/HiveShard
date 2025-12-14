using HiveShard.Interface;

namespace HiveShard.Workers.Initialization.Tests.Initializer;

public class TestShardInitializer: IInitializer
{
    public Task Initialize()
    {
        return Task.CompletedTask;
    }
}