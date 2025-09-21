using System;

namespace HiveShard.Data
{
    [Serializable]
    public class HiveShardClient
    {
        public HiveShardClient(string username)
        {
            Username = username;
        }

        public string Username { get; }

        protected bool Equals(HiveShardClient other)
        {
            return Username == other.Username;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HiveShardClient)obj);
        }

        public override int GetHashCode()
        {
            return (Username != null ? Username.GetHashCode() : 0);
        }
        
        public static bool operator ==(HiveShardClient a, HiveShardClient b)
        {
            throw new InvalidOperationException("Use .Equals instead of ==.");
        }

        public static bool operator !=(HiveShardClient a, HiveShardClient b)
        {
            throw new InvalidOperationException("Use .Equals instead of !=.");
        }
    }
}