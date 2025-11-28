using System;

namespace HiveShard.Ticker
{
    public class TickerConfig
    {
        public int N;
        public Type EventType;

        public TickerConfig(int n, Type eventType)
        {
            N = n;
            EventType = eventType;
        }
    }
}