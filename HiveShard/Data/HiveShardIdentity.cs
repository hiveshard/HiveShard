using System;
using HiveShard.Interface;

namespace HiveShard.Data;

public class HiveShardIdentity: IEventEmitterType
{
    public HiveShardIdentity(Chunk chunk, ShardType shardType, Guid id)
    {
        Chunk = chunk;
        ShardType = shardType;
        Id = id;
    }

    public Chunk Chunk { get; }
    public ShardType ShardType { get; }
    public Guid Id { get; }

    private bool Equals(HiveShardIdentity other)
    {
        return Chunk.Equals(other.Chunk) && ShardType.Equals(other.ShardType) && Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((HiveShardIdentity)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Chunk.GetHashCode();
            hashCode = (hashCode * 397) ^ ShardType.GetHashCode();
            hashCode = (hashCode * 397) ^ Id.GetHashCode();
            return hashCode;
        }
    }

    public EmitterIdentity Identity => new($"{ShardType.TypeName}-{Chunk}");
    public bool InitializationTickOnly => false;
}