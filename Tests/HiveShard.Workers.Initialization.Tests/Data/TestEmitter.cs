using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Workers.Initialization.Tests.Data;

public class TestEmitter: IEventEmitterType
{
    public EmitterIdentity Identity => new("test");
    public bool InitializationTickOnly => false;
}