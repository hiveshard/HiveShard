using HiveShard.Interface;

namespace HiveShard.Data;

public class InitializerType: IEventEmitterType
{
    public InitializerType(EmitterIdentity identity)
    {
        Identity = identity;
    }

    public EmitterIdentity Identity { get; }
    public bool InitializationTickOnly => true;
}