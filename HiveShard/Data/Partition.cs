namespace HiveShard.Data;

public class Partition
{
    public Partition(int value)
    {
        Value = value;
    }

    public int Value { get; }
    
    public Chunk ToChunk(GlobalChunkConfig globalChunkConfig)
    {
        var min = globalChunkConfig.MinChunk;
        var max = globalChunkConfig.MaxChunk;

        var cols = max.YCoord - min.YCoord + 1;

        int x = min.XCoord + (Value / cols);
        int y = min.YCoord + (Value % cols);

        return new Chunk(x, y);
    }
}