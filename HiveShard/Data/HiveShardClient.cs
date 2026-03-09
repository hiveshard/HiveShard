using System;

namespace HiveShard.Data;

[Serializable]
public class HiveShardClient: IEquatable<HiveShardClient>
{

    public HiveShardClient(string username, Guid userId)
    {
        UserId = userId;
        Username = username;
    }

    public string Username { get; }
    public Guid UserId { get; }

    public bool Equals(HiveShardClient? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Username == other.Username && UserId.Equals(other.UserId);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((HiveShardClient)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Username.GetHashCode() * 397) ^ UserId.GetHashCode();
        }
    }

    public static bool operator ==(HiveShardClient a, HiveShardClient? b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(HiveShardClient a, HiveShardClient b)
    {
        return !a.Equals(b);
    }
}