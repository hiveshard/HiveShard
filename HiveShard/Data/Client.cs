using System;

namespace HiveShard.Data
{
    [Serializable]
    public class Client
    {
        public Client(string username)
        {
            Username = username;
        }

        public string Username { get; }

        protected bool Equals(Client other)
        {
            return Username == other.Username;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Client)obj);
        }

        public override int GetHashCode()
        {
            return (Username != null ? Username.GetHashCode() : 0);
        }
        
        public static bool operator ==(Client a, Client b)
        {
            throw new InvalidOperationException("Use .Equals instead of ==.");
        }

        public static bool operator !=(Client a, Client b)
        {
            throw new InvalidOperationException("Use .Equals instead of !=.");
        }
    }
}