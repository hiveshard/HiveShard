using System;

namespace HiveShard.Data
{
    public struct TopicPartitionOffset : IEquatable<TopicPartitionOffset>
    {
        public TopicPartitionOffset(string topic, Chunk partition, long offset)
        {
            Topic = topic;
            Partition = partition;
            Offset = offset;
        }

        public string Topic { get; }
        public Chunk Partition { get; }
        public long Offset { get; }
        
        public bool Equals(TopicPartitionOffset other)
        {
            return Topic == other.Topic && Equals(Partition, other.Partition) && Offset == other.Offset;
        }

        public override bool Equals(object obj)
        {
            return obj is TopicPartitionOffset other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Topic != null ? Topic.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Partition != null ? Partition.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Offset.GetHashCode();
                return hashCode;
            }
        }
    }
}