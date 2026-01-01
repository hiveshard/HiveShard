namespace HiveShard.Data;

public class EmitterIdentity
{
    public EmitterIdentity(string emitterIdentityString)
    {
        EmitterIdentityString = emitterIdentityString;
    }

    public string EmitterIdentityString { get; }

    protected bool Equals(EmitterIdentity other)
    {
        return EmitterIdentityString == other.EmitterIdentityString;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EmitterIdentity)obj);
    }

    public override int GetHashCode()
    {
        return EmitterIdentityString.GetHashCode();
    }
}