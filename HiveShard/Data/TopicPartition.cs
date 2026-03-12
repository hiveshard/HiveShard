using HiveShard.Interface.Config;

namespace HiveShard.Data;

public class TopicPartition
{
    public TopicPartition(string topic, Partition partition)
    {
        Topic = topic;
        Partition = partition;
    }

    public string Topic { get; }
    public Partition Partition { get; }
        
    public string Prefixed(IEnvironmentConfig environmentConfig)
    {
        return $"{environmentConfig.Prefix}-{Topic}";
    }

    private bool Equals(TopicPartition other)
    {
        return Topic == other.Topic && Equals(Partition, other.Partition);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((TopicPartition)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Topic.GetHashCode() * 397) ^ Partition.GetHashCode();
        }
    }
}