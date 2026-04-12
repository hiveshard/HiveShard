using System;
using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;

namespace HiveShard.Workers.Initializer.Data;

public class InitializerEmitterIdentity: IEventEmitterType, IEquatable<InitializerEmitterIdentity>
{
    public InitializerEmitterIdentity(EmitterIdentity identity)
    {
        Identity = identity;
    }
    public static InitializerEmitterIdentity From<T>()
    where T: IInitializer
    {
        return new InitializerEmitterIdentity(new EmitterIdentity(typeof(T).FullName!));
    }
    public EmitterIdentity Identity { get; }
    public bool InitializationTickOnly => true;

    public bool Equals(InitializerEmitterIdentity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Identity.Equals(other.Identity);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((InitializerEmitterIdentity)obj);
    }

    public override int GetHashCode()
    {
        return Identity.GetHashCode();
    }
}