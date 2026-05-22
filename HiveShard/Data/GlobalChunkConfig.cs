using System.Collections.Generic;

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
    public IEnumerable<Chunk> AllChunks => AllChunksList();

    private IEnumerable<Chunk> AllChunksList()
    {
        List<Chunk> list = new List<Chunk>();

        for (int x = MinChunk.XCoord; x <= MaxChunk.XCoord; x++)
        {
            for (int y = MinChunk.YCoord; y <= MaxChunk.YCoord; y++)
            {
                list.Add(new Chunk(x, y));
            }
        }

        return list;
    }
}