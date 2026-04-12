using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Fabrics.InMemory.Tests.Data;

public class TestEmitter: IEventEmitterType
{
    public TestEmitter()
    {
        Identity = new EmitterIdentity("test");
    }

    public EmitterIdentity Identity { get; }
    public bool InitializationTickOnly => false;
}