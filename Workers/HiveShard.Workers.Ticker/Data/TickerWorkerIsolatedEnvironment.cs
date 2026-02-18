using System.Collections.Generic;
using HiveShard.Ticker.Data;

namespace HiveShard.Workers.Ticker.Data;

public class TickerWorkerIsolatedEnvironment : IsolatedEnvironment
{
    public IEnumerable<GlobalTickerIsolatedEnvironment> GlobalTickers { get; }
    public string TickerWorkerIdentifier { get; }
    public IEnumerable<TickerIsolatedEnvironment> Tickers { get; }

    public TickerWorkerIsolatedEnvironment(string tickerWorkerIdentifier,
        IEnumerable<TickerIsolatedEnvironment> tickers, 
        IEnumerable<GlobalTickerIsolatedEnvironment> globalTickers)
    {
        GlobalTickers = globalTickers;
        TickerWorkerIdentifier = tickerWorkerIdentifier;
        Tickers = tickers;
    }

    public override bool IsUnique => false;
}