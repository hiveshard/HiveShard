using System;
using System.Collections.Generic;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Repository
{
    public class ShardRepository: IShardRepository
    {
        private Dictionary<ShardType, Func<Chunk, IHiveShard>> _registeredTypes = new Dictionary<ShardType, Func<Chunk, IHiveShard> >();
        public void RegisterShardType(ShardType shard, Func<Chunk, IHiveShard> instance)
        {
            _registeredTypes[shard] = instance;
        }

        public IEnumerable<Func<Chunk, IHiveShard>> GetShardSuppliers()
            => _registeredTypes.Values;

        public IEnumerable<ShardType> GetShardTypes() => _registeredTypes.Keys;
    }
}