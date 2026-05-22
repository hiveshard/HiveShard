using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Xcepto.Tests.Event;

namespace HiveShard.Xcepto.Tests.Initializer;

public class NoTargetShardInitializer: IInitializer
{
    private GlobalChunkConfig _globalChunkConfig;

    public NoTargetShardInitializer(GlobalChunkConfig globalChunkConfig)
    {
        _globalChunkConfig = globalChunkConfig;
    }

    public void Initialize(IInitializationTunnel tunnel)
    {
        foreach (var chunk in _globalChunkConfig.AllChunks)
        {
            // messages reaches all chunks, but no chunk is targeted
            var noneChunk = new Chunk(int.MinValue, int.MinValue);
            tunnel.Send(new InitializationEvent(noneChunk), chunk);   
        }
    }
}