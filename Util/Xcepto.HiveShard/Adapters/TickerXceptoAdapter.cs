using System;
using System.Threading.Tasks;
using HiveShard.Ticker;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.Adapters
{
    public class TickerXceptoAdapter: XceptoAdapter
    {
        private TickerConfig _tickerConfig;

        public TickerXceptoAdapter(TickerConfig tickerConfig)
        {
            _tickerConfig = tickerConfig;
        }
        protected override Task Initialize(IServiceProvider serviceProvider)
        {
            var ticker = serviceProvider.GetRequiredService<Ticker>();
            var starts = ticker.Start();
            return Task.WhenAll(starts);
        }
    }
}