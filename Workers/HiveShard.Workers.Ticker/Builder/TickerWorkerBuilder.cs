using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Interface;
using HiveShard.Ticker.Data;
using HiveShard.Workers.Ticker.Data;

namespace HiveShard.Workers.Ticker.Builder;

public class TickerWorkerBuilder
{
    private readonly List<TickerIsolatedEnvironment> _tickers = [];
    private readonly List<GlobalTickerIsolatedEnvironment> _globalTickers = [];
    private Guid _tickerIdentifier;

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

    public TickerWorkerBuilder Ticker<T>() where T : class, IEvent => Ticker<T>(Guid.NewGuid());
    
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

    public TickerWorkerBuilder GlobalTicker() => GlobalTicker(new GlobalTickerIdentity(Guid.NewGuid()));

    public TickerWorkerBuilder Identify(Guid tickerIdentifier)
    {
        _tickerIdentifier = tickerIdentifier;
        return this;
    }

}