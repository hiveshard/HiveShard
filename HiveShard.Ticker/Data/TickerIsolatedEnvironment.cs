using System;
using HiveShard.Data;

namespace HiveShard.Ticker.Data;

public class TickerIsolatedEnvironment: IsolatedEnvironment
{
    public Type EventType { get; }

    public TickerIsolatedEnvironment(Type eventType)
    {
        EventType = eventType;
    }
}