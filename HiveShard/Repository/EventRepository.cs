using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Repository;

namespace HiveShard.Repository;

public class EventRepository: IEventRepository
{
    private readonly Dictionary<string, int> _totalOrder = new();
    private readonly Dictionary<string, ISet<IEventEmitterType>> _eventShards = new();
    private readonly Dictionary<EmitterIdentity, ISet<string>> _topicsByEmitter = new();
    private readonly ISet<string> _initializationEvents = new HashSet<string>();
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

        if (shardType.InitializationTickOnly)
            _initializationEvents.Add(type);
        
        if (!_topicsByEmitter.ContainsKey(shardType.Identity))
            _topicsByEmitter[shardType.Identity] = new HashSet<string>();
        _topicsByEmitter[shardType.Identity].Add(type);
            
        return _current;
    }

    public int GetEventOrder<T>() where T : IEvent => GetEventOrder(typeof(T));
    public int GetEventOrder(Type eventType) => GetEventOrder(eventType.FullName!);
    public int GetEventOrder(string eventType) => _totalOrder[eventType];

    public IEnumerable<string> GetInitializationOnlyEvents() => _initializationEvents;

    public KeyValuePair<string, int>[] GetTotalOrder() => _totalOrder.Select(x => x).ToArray();
    public IEventEmitterType[] GetEmitters(string eventType)
    {
        if (!_eventShards.TryGetValue(eventType, out var shard))
            return [];
        return shard.ToArray();
    }

    public string[] GetTopicsOfEmitter(HiveShardIdentity hiveShardIdentity) =>
        _topicsByEmitter[hiveShardIdentity.Identity].ToArray();
}