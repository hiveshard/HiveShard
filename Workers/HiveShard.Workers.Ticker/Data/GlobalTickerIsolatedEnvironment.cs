using HiveShard.Ticker.Data;

namespace HiveShard.Workers.Ticker.Data;

public class GlobalTickerIsolatedEnvironment: IsolatedEnvironment
{
    public GlobalTickerIsolatedEnvironment(GlobalTickerIdentity globalTickerIdentity)
    {
        GlobalTickerIdentity = globalTickerIdentity;
    }

    public override bool IsUnique => false;
    public GlobalTickerIdentity GlobalTickerIdentity { get; }
}