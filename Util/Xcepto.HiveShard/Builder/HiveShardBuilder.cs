using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Interface;
using HiveShard.Ticker;
using Xcepto.HiveShard.Adapters;

namespace Xcepto.HiveShard.Builder
{
    public class HiveShardBuilder
    {
        private int _defaultGridSize = 1;
        private List<Type> _shards = new();

        public HiveShardBuilder AddShard<T>()
        where T: class, IHiveShard
        {
            _shards.Add(typeof(T));
            return this;
        }

        public HiveShardBuilder SetGridSize(int size)
        {
            _defaultGridSize = size;
            return this;
        }

        public HiveShardAdapter Build()
        {
            return new HiveShardAdapter(_defaultGridSize, _shards.AsEnumerable());
        }
    }
}