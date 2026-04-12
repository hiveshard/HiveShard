using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Workers.Ticker.Tests.Data;

public class TestEmitterType: IEventEmitterType
{
    private readonly int _number = 0;
    public TestEmitterType() { }

    public TestEmitterType(int number)
    {
        _number = number;
    }
    public EmitterIdentity Identity => new($"test{_number}");
    public bool InitializationTickOnly => false;
}