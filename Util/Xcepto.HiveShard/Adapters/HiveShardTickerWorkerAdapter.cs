namespace Xcepto.HiveShard.Adapters;
public class HiveShardTickerWorkerAdapter: XceptoAdapter
{
    private string _compartmentIdentifier;

    public HiveShardTickerWorkerAdapter(string identifier)
    {
        _compartmentIdentifier = $"tickerWorker-{identifier}";
    }
}
