using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Ticker.Data;
using HiveShard.Workers.Ticker.Data;

namespace HiveShard.Workers.Ticker.Builder;

public class TickerWorkerBuilder
{
    private List<TickerIsolatedEnvironment> _tickers = new();
    private List<GlobalTickerIsolatedEnvironment> _globalTickers = new();
    private string _tickerIdentifier;

    internal IsolatedEnvironment Build()
    {
        return new TickerWorkerIsolatedEnvironment(_tickerIdentifier, _tickers.AsEnumerable(), _globalTickers.AsEnumerable());
    }

    public TickerWorkerBuilder Ticker<T>(Guid id)
    where T: class, IEvent
    {
        Ticker(new DistributedTickerIdentity(id, typeof(T)));
        return this;
    }
    
    public TickerWorkerBuilder Ticker(DistributedTickerIdentity identity)
    {
        _tickers.Add(new TickerIsolatedEnvironment(identity));
        return this;
    }

    public TickerWorkerBuilder GlobalTicker(GlobalTickerIdentity globalTickerIdentity)
    {
        _globalTickers.Add(new GlobalTickerIsolatedEnvironment(globalTickerIdentity));
        return this;
    }

    public TickerWorkerBuilder Identify(string tickerIdentifier)
    {
        _tickerIdentifier = tickerIdentifier;
        return this;
    }

}