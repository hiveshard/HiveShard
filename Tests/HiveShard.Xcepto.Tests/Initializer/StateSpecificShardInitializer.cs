using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Xcepto.Tests.Event;

namespace HiveShard.Xcepto.Tests.Initializer;

public class StateSpecificShardInitializer: IInitializer
{
    private Chunk _targetChunk;

    public StateSpecificShardInitializer(Chunk targetChunk)
    {
        _targetChunk = targetChunk;
    }

    public void Initialize(IInitializationTunnel tunnel)
    {
        tunnel.Send(new InitializationEvent(_targetChunk), _targetChunk);
    }
}