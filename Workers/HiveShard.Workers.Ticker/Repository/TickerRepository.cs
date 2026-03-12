using System;
using System.Collections.Generic;
using HiveShard.Ticker;
using HiveShard.Ticker.Data;
using HiveShard.Workers.Ticker.Data;

namespace HiveShard.Workers.Ticker.Repository;

public class TickerRepository
{
    private readonly Dictionary<Type, DistributedTicker> _eventTickerMap = new();
    private readonly Dictionary<GlobalTickerIdentity, GlobalTicker> _globalTickerMap = new();

    public void AddTicker(Type eventType, DistributedTicker eventTicker)
    {
        _eventTickerMap.Add(eventType, eventTicker);
    }
    
    public void AddGlobalTicker(GlobalTickerIdentity globalTickerIdentity, GlobalTicker globalTicker)
    {
        _globalTickerMap.Add(globalTickerIdentity, globalTicker);
    }

    public DistributedTicker GetTicker(Type eventType)
    {
        return _eventTickerMap[eventType];
    }

    public IEnumerable<DistributedTicker> GetAll() => _eventTickerMap.Values;

    public IEnumerable<GlobalTicker> GetGlobalTickers() => _globalTickerMap.Values;
}