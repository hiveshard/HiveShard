namespace HiveShard.Data;

public class GlobalChunkConfig
{
    public GlobalChunkConfig(Chunk minChunk, Chunk maxChunk)
    {
        MinChunk = minChunk;
        MaxChunk = maxChunk;
    }

    public Chunk MinChunk { get; }
    public Chunk MaxChunk { get; }
}