namespace HiveShard.Data
{
    public class HiveShardIdentity
    {
        public HiveShardIdentity(Chunk chunk, ShardType shardType)
        {
            Chunk = chunk;
            ShardType = shardType;
        }

        public Chunk Chunk { get; }
        public ShardType ShardType { get; }

        private bool Equals(HiveShardIdentity other)
        {
            return Equals(Chunk, other.Chunk) && Equals(ShardType, other.ShardType);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HiveShardIdentity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Chunk != null ? Chunk.GetHashCode() : 0) * 397) ^ (ShardType != null ? ShardType.GetHashCode() : 0);
            }
        }
    }

}