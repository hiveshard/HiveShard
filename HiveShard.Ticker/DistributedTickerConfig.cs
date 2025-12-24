using System;
using HiveShard.Data;

namespace HiveShard.Ticker;

public class DistributedTickerConfig
{
    public Type EventType { get; }

    public DistributedTickerConfig(Type eventType, Chunk[] chunks)
    {
        this.EventType = eventType;
    }
}