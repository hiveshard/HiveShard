using HiveShard.Ticker;

namespace Xcepto.HiveShard.Adapters;
public class HiveShardTickerWorkerAdapter: XceptoAdapter
{
    private TickerConfig _tickerConfig;
    private string _compartmentIdentifier;

    public HiveShardTickerWorkerAdapter(TickerConfig tickerConfig, string identifier)
    {
        _tickerConfig = tickerConfig;
        _compartmentIdentifier = $"tickerWorker-{identifier}";
    }
}
