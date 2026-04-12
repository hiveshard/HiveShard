using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Workers.Ticker.Tests.Data;

public class DistributedTickerType: IEventEmitterType
{
    private readonly int _number = 0;
    public DistributedTickerType() { }

    public DistributedTickerType(int number)
    {
        _number = number;
    }
    public EmitterIdentity Identity => new($"test{_number}");
    public bool InitializationTickOnly => false;
}