using System;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Fabrics.InMemory.Data;

public class DelayedConsumption
{
    public DelayedConsumption(Action<Consumption<IEnvelope<object>>> action, Consumption<IEnvelope<object>> value, long consumedOffset, long newOffset, string topic, Partition partition, EmitterIdentity consumer)
    {
        Action = action;
        Value = value;
        ConsumedOffset = consumedOffset;
        NewOffset = newOffset;
        Topic = topic;
        Partition = partition;
        Consumer = consumer;
    }

    public Action<Consumption<IEnvelope<object>>> Action { get; }
    public Consumption<IEnvelope<object>> Value { get; } 
    public long ConsumedOffset { get; }
    public long NewOffset { get; }
    public string Topic { get; }
    public Partition Partition { get; }
    public EmitterIdentity Consumer { get; }
}