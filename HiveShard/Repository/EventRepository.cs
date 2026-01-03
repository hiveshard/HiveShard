using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Repository;

namespace HiveShard.Repository;

public class EventRepository: IEventRepository
{
    private Dictionary<string, int> _totalOrder = new Dictionary<string, int>();
    private Dictionary<string, ISet<IEventEmitterType>> _eventShards = new Dictionary<string, ISet<IEventEmitterType>>();
    private Dictionary<EmitterIdentity, ISet<string>> _topicsByEmitter = new();
    private int _current = 0;
    public int RegisterEvent<T>(IEventEmitterType shardType)
        where T : IEvent
    {
        var type = typeof(T).FullName!;
        if(!_totalOrder.ContainsKey(type))
        {
            _totalOrder[type] = ++_current;
            _eventShards[type] = new HashSet<IEventEmitterType>();
        }
        _eventShards[type].Add(shardType);

        if (!_topicsByEmitter.ContainsKey(shardType.Identity))
            _topicsByEmitter[shardType.Identity] = new HashSet<string>();
        _topicsByEmitter[shardType.Identity].Add(type);
            
        return _current;
    }

    public int GetEventOrder<T>() where T : IEvent => GetEventOrder(typeof(T));
    public int GetEventOrder(Type eventType) => GetEventOrder(eventType.FullName!);
    public int GetEventOrder(string eventType) => _totalOrder[eventType];

    public KeyValuePair<string, int>[] GetTotalOrder() => _totalOrder.Select(x => x).ToArray();
    public IEventEmitterType[] GetEmitters(string eventType)
    {
        if (!_eventShards.ContainsKey(eventType))
            return [];
        return _eventShards[eventType].ToArray();
    }

    public string[] GetTopicsOfEmitter(HiveShardIdentity hiveShardIdentity) =>
        _topicsByEmitter[hiveShardIdentity.Identity].ToArray();
}