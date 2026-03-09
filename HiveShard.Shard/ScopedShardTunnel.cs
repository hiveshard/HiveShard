using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Shard.Data;
using HiveShard.Shard.Interfaces;

namespace HiveShard.Shard;

public class ScopedShardTunnel: IScopedShardTunnel
{
    private Dictionary<string, Tick> _advancedTicks = new();
    private Dictionary<TopicPartition, long> _consumedOffsets = new();
    private Dictionary<string, Action<Message<object>>> _registrations = new();
    private HiveShardIdentity _identity;
    private GlobalChunkConfig _globalChunkConfig;
    private ISimpleFabric _fabric;

    public ScopedShardTunnel(ISimpleFabric fabric, GlobalChunkConfig globalChunkConfig)
    {
        _fabric = fabric;
        _globalChunkConfig = globalChunkConfig;
    }

    private void AdvanceTick(Tick tick)
    {
        if (_identity == null)
            throw new Exception("not initialized yet");
                
        if (!_advancedTicks.ContainsKey(tick.TickEventType))
            _advancedTicks[tick.TickEventType] = tick;
        else if(_advancedTicks[tick.TickEventType].TickNumber >= tick.TickNumber)
            return;
        
        _advancedTicks[tick.TickEventType] = tick;

        if (_registrations.Keys.All(type => _advancedTicks.TryGetValue(type, out var t) 
                                            && t.TickNumber == tick.TickNumber))
            AdvanceShard(_advancedTicks.Values.OrderBy(x=> x.TickEventType));
    }

    private void AdvanceShard(IEnumerable<Tick> ticks)
    {
        foreach (var tick in ticks)
        {
            foreach (var chunk in _identity.Chunk.GetNeighboursAndSelf(_globalChunkConfig))
            {
                var topicPartition = new TopicPartition(tick.TickEventType, chunk);
                if (!_consumedOffsets.ContainsKey(topicPartition))
                    _consumedOffsets[topicPartition] = 0;

                var registration = _registrations[tick.TickEventType];
                long toOffset = tick.ChunkOffsets
                    .Where(x => x.Partition.Equals(chunk))
                    .Select(x => x.Offset)
                    .FirstOrDefault(); // default = 0 is fine

                foreach (var message in _fabric.FetchTopic(topicPartition, _consumedOffsets[topicPartition], toOffset))
                {
                    registration(message);
                }

                _consumedOffsets[topicPartition] = toOffset;
            }
        }
    }
    
    public void Register<TEvent>(Action<Message<TEvent>> handler) where TEvent : IEvent
    {
        _registrations.Add(typeof(TEvent).FullName!, consumption =>
        {
            handler(new Message<TEvent>((TEvent)consumption.Payload, consumption.Chunk));
        });
    }

    public void Send<TEvent>(TEvent message) where TEvent : IEvent
    {
        _fabric.Send(typeof(TEvent).FullName!, _identity.Chunk, message);
    }

    public void Initialize(IHiveShard shard, HiveShardIdentity identity)
    {
        _identity = identity;
        shard.Initialize(_identity.Chunk);
        _fabric.Register<Tick>("ticks", x => AdvanceTick(x.Message));
    }
}