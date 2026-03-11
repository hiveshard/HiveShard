using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Ticker.Tests.Data;

public class TestEmitterType: IEventEmitterType
{
    public EmitterIdentity Identity => new("test");
    public bool InitializationTickOnly => false;
}