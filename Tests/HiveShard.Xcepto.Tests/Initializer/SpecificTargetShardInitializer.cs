using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Xcepto.Tests.Event;

namespace HiveShard.Xcepto.Tests.Initializer;

public class SpecificTargetShardInitializer: IInitializer
{
    private Chunk _targetChunk;
    private GlobalChunkConfig _globalChunkConfig;

    public SpecificTargetShardInitializer(Chunk targetChunk, GlobalChunkConfig globalChunkConfig)
    {
        _targetChunk = targetChunk;
        _globalChunkConfig = globalChunkConfig;
    }

    public void Initialize(IInitializationTunnel tunnel)
    {
        foreach (var chunk in _globalChunkConfig.AllChunks)
        {
            tunnel.Send(new InitializationEvent(_targetChunk), chunk);
        }
    }
}