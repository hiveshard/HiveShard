using System;
using System.Collections.Generic;
using HiveShard.Interface;
using HiveShard.Ticker;
using HiveShard.Workers.Ticker.Data;

namespace HiveShard.Workers.Ticker.Repository;

public class TickerRepository
{
    private readonly Dictionary<Type, EventTickerInstance> _map = new();

    public void AddTicker(Type eventType, EventTickerInstance eventTicker)
    {
        _map.Add(eventType, eventTicker);
    }

    public EventTickerInstance GetTicker(Type eventType)
    {
        return _map[eventType];
    }

    public IEnumerable<EventTickerInstance> GetAll() => _map.Values;
}