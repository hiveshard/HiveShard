using System;
using HiveShard.Workers.Ticker.Data;

namespace HiveShard.Workers.Ticker;

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