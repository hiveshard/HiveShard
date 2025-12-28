using System;

namespace HiveShard.Ticker
{
    public class TickerConfig
    {
        public Type EventType;

        public TickerConfig(Type eventType)
        {
            EventType = eventType;
        }
    }
}