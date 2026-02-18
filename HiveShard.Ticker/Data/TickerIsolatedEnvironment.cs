namespace HiveShard.Ticker.Data;

public class TickerIsolatedEnvironment: IsolatedEnvironment
{

    public TickerIsolatedEnvironment(DistributedTickerIdentity tickerIdentity)
    {
        Identity = tickerIdentity;
    }

    public override bool IsUnique => false;
    public DistributedTickerIdentity Identity { get; }
}