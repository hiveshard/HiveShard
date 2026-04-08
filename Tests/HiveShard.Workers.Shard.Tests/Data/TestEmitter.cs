using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Worker.Tests.Data;

public class TestEmitter: IEventEmitterType
{
    public EmitterIdentity Identity => new EmitterIdentity("test");
    public bool InitializationTickOnly => false;
}