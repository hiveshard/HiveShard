using HiveShard.Data;

namespace HiveShard.Interface;

public interface IEventEmitterType
{
    public EmitterIdentity Identity { get; }
    public bool InitializationTickOnly { get; }
}