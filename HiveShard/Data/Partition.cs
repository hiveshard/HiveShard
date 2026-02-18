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

        var x = min.XCoord + Value / cols;
        var y = min.YCoord + Value % cols;

        return new Chunk(x, y);
    }

    private bool Equals(Partition other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Partition)obj);
    }

    public override int GetHashCode()
    {
        return Value;
    }
}