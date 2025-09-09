using System;
using System.Collections.Generic;
using HiveShard.Data;

namespace HiveShard.Interface
{
    public interface IShardRepository
    {
        public void RegisterShardType(ShardType shard, Func<Chunk, IHiveShard> instance);
        IEnumerable<Func<Chunk, IHiveShard>> GetShardSuppliers();
        IEnumerable<ShardType> GetShardTypes();
    }
}