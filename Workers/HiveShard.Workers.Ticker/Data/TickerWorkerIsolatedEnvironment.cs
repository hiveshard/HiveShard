using HiveShard.Ticker.Data;

namespace HiveShard.Workers.Ticker.Data
{
    public class TickerWorkerIsolatedEnvironment : IsolatedEnvironment
    {
        public string TickerWorkerIdentifier { get; }
        public IEnumerable<TickerIsolatedEnvironment> Tickers { get; }

        public TickerWorkerIsolatedEnvironment(string tickerWorkerIdentifier, IEnumerable<TickerIsolatedEnvironment> tickers)
        {
            TickerWorkerIdentifier = tickerWorkerIdentifier;
            Tickers = tickers;
        }
    }
}