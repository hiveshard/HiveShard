using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Repository;

namespace HiveShard.Repository;

public class EventRepository: IEventRepository
{
    private Dictionary<Type, int> _totalOrder = new Dictionary<Type, int>();
    private Dictionary<Type, ISet<ShardType>> _eventShards = new Dictionary<Type, ISet<ShardType>>();
    private int _current = 0;
    public int RegisterEvent<T>(ShardType shardType)
        where T : IEvent
    {
        var type = typeof(T);
        if(!_totalOrder.ContainsKey(type))
        {
            _totalOrder[type] = ++_current;
            _eventShards[type] = new HashSet<ShardType>();
        }

        _eventShards[type].Add(shardType);
        return _current;
    }

    public int GetEventOrder<T>() where T : IEvent => GetEventOrder(typeof(T));
    public int GetEventOrder(Type eventType) => _totalOrder[eventType];

    public KeyValuePair<Type, int>[] GetTotalOrder() => _totalOrder.Select(x => x).ToArray();
}