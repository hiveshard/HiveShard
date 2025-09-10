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
            return Task.CompletedTask;
        }

        protected override Task AddServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Ticker>();
            serviceCollection.AddSingleton<TickerConfig>(_tickerConfig);
            return Task.CompletedTask;
        }
    }
}