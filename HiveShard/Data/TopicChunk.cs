using HiveShard.Interface.Config;

namespace HiveShard.Data;

public class TopicChunk
{
    public TopicChunk(string topic, Chunk chunk)
    {
        Topic = topic;
        Chunk = chunk;
    }

    public string Topic { get; }
    public Chunk Chunk { get; }
        
    public string Prefixed(IEnvironmentConfig environmentConfig)
    {
        return $"{environmentConfig.Prefix}-{Topic}";
    }

    private bool Equals(TopicChunk other)
    {
        return Topic == other.Topic && Equals(Chunk, other.Chunk);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((TopicChunk)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Topic.GetHashCode() * 397) ^ Chunk.GetHashCode();
        }
    }
}