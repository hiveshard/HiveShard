using System;
using HiveShard.Data;

namespace HiveShard.Workers.Ticker.Data;

public class GlobalTickerIdentity
{
    public GlobalTickerIdentity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }

    protected bool Equals(GlobalTickerIdentity other)
    {
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((GlobalTickerIdentity)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public EmitterIdentity ToEmitterType()
    {
        return new EmitterIdentity($"globalTicker[{Id}]");
    }
}