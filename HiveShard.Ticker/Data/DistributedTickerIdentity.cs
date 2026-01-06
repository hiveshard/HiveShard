using System;
using HiveShard.Data;

namespace HiveShard.Ticker.Data;

public class DistributedTickerIdentity
{

    public DistributedTickerIdentity(Guid id, Type eventType)
    {
        Id = id;
        EventType = eventType;
    }

    public Guid Id { get; }
    public Type EventType { get; }

    protected bool Equals(DistributedTickerIdentity other)
    {
        return Id.Equals(other.Id) && EventType.Equals(other.EventType);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DistributedTickerIdentity)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Id.GetHashCode() * 397) ^ EventType.GetHashCode();
        }
    }

    public EmitterIdentity ToEmitterIdentity()
    {
        return new EmitterIdentity($"{EventType.FullName!}[{Id}]");
    }
}