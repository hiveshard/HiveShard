using HiveShard.Data;

namespace HiveShard.Propagation.Tests.Config;

public class SingleChunkInitializerConfig
{
    public SingleChunkInitializerConfig(int secret, Chunk targetChunk)
    {
        Secret = secret;
        TargetChunk = targetChunk;
    }

    public int Secret { get; }
    public Chunk TargetChunk { get; }
}