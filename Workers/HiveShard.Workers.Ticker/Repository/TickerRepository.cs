using System;
using System.Collections.Generic;
using HiveShard.Interface;
using HiveShard.Ticker;
using HiveShard.Ticker.Data;
using HiveShard.Workers.Ticker.Data;

namespace HiveShard.Workers.Ticker.Repository;

public class TickerRepository
{
    private readonly Dictionary<Type, EventTickerInstance> _eventTickerMap = new();
    private readonly Dictionary<GlobalTickerIdentity, GlobalTickerInstance> _globalTickerMap = new();

    public void AddTicker(Type eventType, EventTickerInstance eventTicker)
    {
        _eventTickerMap.Add(eventType, eventTicker);
    }
    
    public void AddGlobalTicker(GlobalTickerIdentity globalTickerIdentity, GlobalTickerInstance globalTicker)
    {
        _globalTickerMap.Add(globalTickerIdentity, globalTicker);
    }

    public EventTickerInstance GetTicker(Type eventType)
    {
        return _eventTickerMap[eventType];
    }

    public IEnumerable<EventTickerInstance> GetAll() => _eventTickerMap.Values;

    public IEnumerable<GlobalTickerInstance> GetGlobalTickers() => _globalTickerMap.Values;
}