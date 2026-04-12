using HiveShard.Interface.Config;

namespace HiveShard.Workers.Edge.Tests.Provider;

public class RandomNetworkConfigurationProvider: INetworkConfiguration
{
    private static readonly Random _random = new(3000);

    private readonly int _port; 
    public RandomNetworkConfigurationProvider()
    {
        _port = _random.Next(3000, 4000);
    }

    public int Port() => _port;
}