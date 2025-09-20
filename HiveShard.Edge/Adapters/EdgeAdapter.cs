using System;
using HiveShard.Builder;
using Xcepto.HiveShard.Builder;

namespace HiveShard.Edge.Adapters;

public class EdgeAdapter: HiveShardServiceAdapter
{
    public void AddEdgeWorker(Func<EdgeWorkerBuilder, EdgeWorkerEnvironment> func)
    {
        throw new NotImplementedException();
    }
}