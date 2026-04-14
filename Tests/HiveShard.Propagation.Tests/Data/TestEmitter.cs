using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Propagation.Tests.Data;

public class TestEmitter: IEventEmitterType
{
    public EmitterIdentity Identity => new("test");
    public bool InitializationTickOnly => false;
}