using System;

namespace HiveShard.Ticker;

public class DistributedTickerConfig
{
    public Type EventType { get; }

    public DistributedTickerConfig(Type eventType)
    {
        this.EventType = eventType;
    }
}