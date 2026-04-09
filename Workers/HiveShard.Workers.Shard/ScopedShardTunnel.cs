using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Repository;
using HiveShard.Shard.Interfaces;
using HiveShard.Workers.Shard.Data;

namespace HiveShard.Workers.Shard;

public class ScopedShardTunnel: IScopedShardTunnel
{
    private Dictionary<string, Tick> _advancedTicks = new();
    private Dictionary<TopicChunk, long> _consumedOffsets = new();
    private Dictionary<string, Action<ShardRegistrationContext>> _registrations = new();
    private HiveShardIdentity _identity;
    private GlobalChunkConfig _globalChunkConfig;
    private ISimpleFabric _fabric;
    private IHiveShardTelemetry _telemetry;
    private IEventRepository _eventRepository;

    public ScopedShardTunnel(ISimpleFabric fabric, GlobalChunkConfig globalChunkConfig, IHiveShardTelemetry telemetry, IEventRepository eventRepository)
    {
        _fabric = fabric;
        _globalChunkConfig = globalChunkConfig;
        _telemetry = telemetry;
        _eventRepository = eventRepository;
    }

    private long _lastAdvancedTick = -1;
    
    private void AdvanceTick(Tick tick)
    {
        if (_identity == null)
            throw new Exception("not initialized yet");
                
        if (!_advancedTicks.ContainsKey(tick.TickEventType))
            _advancedTicks[tick.TickEventType] = tick;
        else if(_advancedTicks[tick.TickEventType].TickNumber >= tick.TickNumber)
            return;
        
        _advancedTicks[tick.TickEventType] = tick;

        if (_requiredEvents.All(type => _advancedTicks.TryGetValue(type, out var t) 
                                        && t.TickNumber == tick.TickNumber))
        {
            if (_lastAdvancedTick >= tick.TickNumber)
                return;

            _lastAdvancedTick = tick.TickNumber;
            
            AdvanceShard(_advancedTicks.Values
                .Where(x => _registrations.ContainsKey(x.TickEventType)) // only READ event type ticks are relevant
                .OrderBy(x => x.TickEventType));
        }
    }

    private void AdvanceShard(IEnumerable<Tick> ticks)
    {
        foreach (var tick in ticks)
        {
            foreach (var chunk in _identity.Chunk.GetNeighboursAndSelf(_globalChunkConfig))
            {
                var topicPartition = new TopicChunk(tick.TickEventType, chunk);
                if (!_consumedOffsets.ContainsKey(topicPartition))
                    _consumedOffsets[topicPartition] = 0;

                var registration = _registrations[tick.TickEventType];
                long toOffset = tick.ChunkOffsets
                    .Where(x => x.Partition.Equals(chunk))
                    .Select(x => x.Offset)
                    .FirstOrDefault(); // default = 0 is fine

                foreach (var consumption in _fabric.FetchTopic(topicPartition, _consumedOffsets[topicPartition], toOffset))
                {
                    registration(new ShardRegistrationContext(consumption, tick.TickNumber, topicPartition));
                }

                _consumedOffsets[topicPartition] = toOffset;
            }

            foreach (var topic in _eventRepository.GetTopicsOfEmitter(_identity.Identity))
            {
                _fabric.Send(typeof(CompletedTick).FullName!, new Partition(_eventRepository.GetEventOrder(topic)),
                    results =>
                    {
                        List<TopicPartitionOffset> offsets = results.Offsets
                            .Select(x => new TopicPartitionOffset(
                                x.Key.Topic, 
                                x.Key.Partition.ToChunk(_globalChunkConfig), 
                                x.Value
                            )
                        ).ToList();
                        return new Envelope<CompletedTick>(
                            new CompletedTick(_identity.Identity, tick.TickNumber, topic, offsets),
                            Guid.NewGuid()
                        );
                    }
                );   
            }
        }
    }

    private List<string> _requiredEvents = new();
    private ShardRegistrationContext? _currentContext;
    public void Register<TEvent>(Action<Message<TEvent>> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent).FullName!;
        _registrations.Add(eventType, context =>
        {
            _currentContext = context;
            handler(new Message<TEvent>((TEvent)context.Consumption.Message.Payload, context.TopicChunk.Chunk));
        });
        _requiredEvents.Add(eventType);
    }

    public void Send<TEvent>(TEvent message) where TEvent : IEvent
    {
        if (_currentContext is null)
            throw new Exception("Send without context is impossible");
        IEnvelope<TEvent> envelope = new Envelope<TEvent>(message, Guid.NewGuid());
        _fabric.Send(typeof(TEvent).FullName!, _identity.Chunk, envelope);
        _telemetry.Cause(new TransitionCause(
            tick: _currentContext.Tick,
            shardType: _identity.ShardType.TypeName,
            converge: 0,
            shardX: _identity.Chunk.XCoord,
            shardY: _identity.Chunk.YCoord,
            shardReplica: 1,
            inboundEvent: _currentContext.Consumption.Message.MessageId,
            inboundEventType: _currentContext.TopicChunk.Topic,
            outboundEvent: envelope.MessageId,
            outboundEventType: typeof(TEvent).FullName!
        ));
    }

    public void Initialize(IHiveShard shard, HiveShardIdentity identity)
    {
        _identity = identity;
        shard.Initialize(_identity.Chunk);
        
        // wait for both input-event and output-event based tick events 
        _requiredEvents.AddRange(_eventRepository.GetTopicsOfEmitter(_identity.Identity));
        
        foreach (var requiredEvent in _requiredEvents)
        {
            var partition = _eventRepository.GetEventOrder(requiredEvent);
            _fabric.Register<Tick>(typeof(Tick).FullName!, new Partition(partition), x => AdvanceTick(x.Message.Payload));
        }
    }
}