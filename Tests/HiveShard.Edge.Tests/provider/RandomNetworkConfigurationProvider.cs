using HiveShard.Interface;

namespace HiveShard.Edge.Tests.provider;

public class RandomNetworkConfigurationProvider: INetworkConfiguration
{
    private static Random _random = new(3000);

    private readonly int _port; 
    public RandomNetworkConfigurationProvider()
    {
        _port = _random.Next(3000, 4000);
    }

    public int Port() => _port;
}