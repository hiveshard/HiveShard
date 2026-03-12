using System;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Shard.Data;

public class ShardRegistrationContext
{

    public ShardRegistrationContext(Consumption<IEnvelope<object>> consumption, long tick, TopicChunk topicChunk)
    {
        TopicChunk = topicChunk;
        Consumption = consumption;
        Tick = tick;
    }

    public Consumption<IEnvelope<object>> Consumption { get; }
    public long Tick { get; }
    public TopicChunk TopicChunk { get; }
}