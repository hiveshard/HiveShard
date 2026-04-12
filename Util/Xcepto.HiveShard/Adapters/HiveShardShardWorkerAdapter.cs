using Xcepto.Adapters;

namespace Xcepto.HiveShard.Adapters;

public class HiveShardShardWorkerAdapter:XceptoAdapter
{
    private string _compartmentIdentifier;

    public HiveShardShardWorkerAdapter(string worker)
    {
        _compartmentIdentifier = $"shardWorker-{worker}";
    }
        
}