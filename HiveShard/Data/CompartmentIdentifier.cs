using System;

namespace HiveShard.Data;

public class CompartmentIdentifier : IEquatable<CompartmentIdentifier>
{
    public CompartmentIdentifier(Guid id, CompartmentType compartmentType)
    {
        Id = id;
        CompartmentType = compartmentType;
    }

    public Guid Id { get; }
    public CompartmentType CompartmentType { get; }

    public bool Equals(CompartmentIdentifier? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && CompartmentType == other.CompartmentType;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((CompartmentIdentifier)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Id.GetHashCode() * 397) ^ CompartmentType.GetHashCode();
        }
    }

    public override string ToString()
    {
        return $"{CompartmentType}-{Id:N}";
    }
}