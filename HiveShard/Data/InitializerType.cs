using HiveShard.Interface;

namespace HiveShard.Data;

public class InitializerType: IEventEmitterType
{
    public InitializerType(string identity)
    {
        Identity = identity;
    }

    public string Identity { get; }
    public bool EmitsFirstTickOnly => true;
}