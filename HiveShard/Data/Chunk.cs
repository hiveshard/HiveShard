using System.Collections.Generic;

namespace HiveShard.Data;

public class Chunk
{
    public int XCoord { get; }
    public int YCoord { get; }

    public Chunk(int xCoord, int yCoord)
    {
        XCoord = xCoord;
        YCoord = yCoord;
    }

    public string Topic => $"{XCoord}-{YCoord}";

    public IEnumerable<Chunk> GetNeighboursAndSelf(GlobalChunkConfig globalChunkConfig)
    {
        for (var i = -1; i <= 1; i++)
        for (var j = -1; j <= 1; j++)
        {
            var x = XCoord + i;
            var y = YCoord + j;

            if (x < globalChunkConfig.MinChunk.XCoord ||
                y < globalChunkConfig.MinChunk.YCoord ||
                x > globalChunkConfig.MaxChunk.XCoord ||
                y > globalChunkConfig.MaxChunk.YCoord)
                continue;

            yield return new Chunk(x, y);
        }
    }

    protected bool Equals(Chunk other)
    {
        return XCoord == other.XCoord && YCoord == other.YCoord;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Chunk)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (XCoord * 397) ^ YCoord;
        }
    }

    public Partition ToPartition(GlobalChunkConfig globalChunkConfig)
    {
        var max = globalChunkConfig.MaxChunk;
        var min = globalChunkConfig.MinChunk;
        var cols = max.YCoord - min.YCoord + 1;
        return new Partition((XCoord - min.XCoord) * cols + (YCoord - min.YCoord));
    }

    public override string ToString()
    {
        return Topic;
    }
}