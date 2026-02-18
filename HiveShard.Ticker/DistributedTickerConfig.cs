using System;
using HiveShard.Ticker.Data;

namespace HiveShard.Ticker;

public class DistributedTickerConfig
{
    public Type EventType { get; }
    public TickerEmitterType EmitterType { get; }

    public DistributedTickerConfig(Type eventType, TickerEmitterType emitterType)
    {
        this.EventType = eventType;
        EmitterType = emitterType;
    }
}