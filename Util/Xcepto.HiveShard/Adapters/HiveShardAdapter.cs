using System;
using System.Collections.Generic;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardAdapter: XceptoAdapter
    {
        private int _defaultGridSize;
        private IEnumerable<Type> _shardTypes;

        public HiveShardAdapter(int defaultGridSize, IEnumerable<Type> shardTypes)
        {
            _shardTypes = shardTypes;
            _defaultGridSize = defaultGridSize;
        }

        public void SendEdgeMessage()
        {
            throw new System.NotImplementedException();
        }

        public void SendShardMessage()
        {
            throw new System.NotImplementedException();
        }
    }
}