using System;
using System.Collections.Generic;
using HiveShard.Ticker;
using HiveShard.Ticker.Data;
using HiveShard.Workers.Ticker.Data;

namespace HiveShard.Workers.Ticker.Repository;

public class TickerRepository
{
    private readonly Dictionary<Type, DistributedTicker> _eventTickerMap = new();
    private readonly Dictionary<GlobalTickerIdentity, TickBarrier> _globalTickerMap = new();

    public void AddTicker(Type eventType, DistributedTicker eventTicker)
    {
        _eventTickerMap.Add(eventType, eventTicker);
    }
    
    public void AddGlobalTicker(GlobalTickerIdentity globalTickerIdentity, TickBarrier tickBarrier)
    {
        _globalTickerMap.Add(globalTickerIdentity, tickBarrier);
    }

    public DistributedTicker GetTicker(Type eventType)
    {
        return _eventTickerMap[eventType];
    }

    public IEnumerable<DistributedTicker> GetAll() => _eventTickerMap.Values;

    public IEnumerable<TickBarrier> GetGlobalTickers() => _globalTickerMap.Values;
}