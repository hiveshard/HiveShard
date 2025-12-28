using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using Xcepto.Adapters;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardShardWorkerAdapter:XceptoAdapter
    {
        private string _compartmentIdentifier;

        public HiveShardShardWorkerAdapter(string worker)
        {
            _compartmentIdentifier = $"shardWorker-{worker}";
        }
        
    }
}