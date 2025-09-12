using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Factory;

public class HiveShardFactory
{
    private ServiceCollection _serviceCollection;

    public HiveShardFactory()
    {
        _serviceCollection = new ServiceCollection();
    }
    public ServiceCollection Build()
    {
        return _serviceCollection;
    }
}