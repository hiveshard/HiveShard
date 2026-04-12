using System.Collections.Generic;

namespace HiveShard.Data;

public class BatchedOffsetResults
{
    public BatchedOffsetResults(IReadOnlyDictionary<TopicPartition, long> offsets)
    {
        Offsets = offsets;
    }

    public IReadOnlyDictionary<TopicPartition, long> Offsets { get; }
}