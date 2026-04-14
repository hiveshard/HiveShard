using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Validation.Tests.Events;

namespace HiveShard.Validation.Tests.Initializer;

public class PlainInitializer: IInitializer
{
    private GlobalChunkConfig _chunkConfig;

    public PlainInitializer(GlobalChunkConfig chunkConfig)
    {
        _chunkConfig = chunkConfig;
    }

    public void Initialize(IInitializationTunnel tunnel)
    {
        for (int x = _chunkConfig.MinChunk.XCoord; x <= _chunkConfig.MaxChunk.XCoord; x++)
        {
            for (int y = _chunkConfig.MinChunk.YCoord; y <= _chunkConfig.MaxChunk.YCoord; y++)
            {
                var chunk = new Chunk(x, y);
                tunnel.Send(new InputOnlyInitEvent(), chunk);
            }
        }
    }
}