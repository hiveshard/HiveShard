using System;
using HiveShard.Interface;

namespace HiveShard.Data
{
    public class ShardType
    {
        private ShardType(string typeName)
        {
            TypeName = typeName;
        }

        public static ShardType From<T>()
            where T : class, IHiveShard
        {
            return new ShardType(typeof(T).AssemblyQualifiedName!);
        }

        public string TypeName { get; }

        public Type GetShardType()
        {
            var shardType = Type.GetType(TypeName);
            if (shardType is null)
                throw new Exception($"Type not found: {TypeName}");
            return shardType;
        }

        protected bool Equals(ShardType other)
        {
            return TypeName == other.TypeName;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ShardType)obj);
        }

        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
        }
    }
}