using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Ticker.Tests.Data;

public class TestInitializerEmitterType: IEventEmitterType
{
    public EmitterIdentity Identity => new("initializer");
    public bool InitializationTickOnly => true;
}