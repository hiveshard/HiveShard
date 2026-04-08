using System.Collections.Generic;
using HiveShard.Event;

namespace HiveShard.Workers.Ticker.Util;

public sealed class CompletedTickComparer : IEqualityComparer<CompletedTick>
{
    public bool Equals(CompletedTick? x, CompletedTick? y)
        => ReferenceEquals(x, y)
           || (x is not null && y is not null
                             && x.Tick == y.Tick
                             && x.EventType == y.EventType);

    public int GetHashCode(CompletedTick obj)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + obj.Tick.GetHashCode();
            hash = hash * 31 + (obj.EventType?.GetHashCode() ?? 0);
            return hash;
        }
    }

}
