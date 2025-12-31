using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Data.Telemetry;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Providers;
using HiveShard.Interface.Repository;
using HiveShard.Shard.Data;
using HiveShard.Shard.Interfaces;
using HiveShard.Util;

namespace HiveShard.Shard
{
    public class ScopedShardTunnel: IScopedShardTunnel
    {
        private readonly HiveShardIdentity _hiveShardIdentity;
        private readonly ISimpleFabric _simpleFabric;
        private readonly IWorkerLoggingProvider _loggingProvider;
        private readonly ICancellationProvider _cancellationProvider;
        private readonly Dictionary<TopicPartition, BlockingCollection<Caster>> _events = new();
        private readonly BlockingCollection<Consumption<Tick>> _ticks = new(50);
        private readonly Dictionary<TopicPartition, long> _eventQueueOffsets = new();
        private ITickRepository _tickRepository;
        private GlobalChunkConfig _globalChunkConfig;
        private IEventRepository _eventRepository;

        private long _lastTick;
        private volatile int _ready;
        private IHiveShard? _hiveShard;
        
        public ScopedShardTunnel(HiveShardIdentity hiveShardIdentity, IWorkerLoggingProvider loggingProvider, ISimpleFabric simpleFabric, ITickRepository tickRepository, ICancellationProvider cancellationProvider, GlobalChunkConfig globalChunkConfig, IEventRepository eventRepository)
        {
            _simpleFabric = simpleFabric;
            _tickRepository = tickRepository;
            _cancellationProvider = cancellationProvider;
            _globalChunkConfig = globalChunkConfig;
            _eventRepository = eventRepository;
            _hiveShardIdentity = hiveShardIdentity;
            _loggingProvider = loggingProvider;
            
            simpleFabric.Register<Tick>("ticks", x =>
            {
                _ticks.Add(new Consumption<Tick>(x.Message, x.Offset));
            });
        }
        
        public void Initialize<T>(T hiveShard) 
            where T : class, IHiveShard
        {
            _hiveShard = hiveShard;
            _hiveShard.Initialize();
        }



        public Task Start()
        {
            if (_hiveShard is null)
                throw new Exception("Not initialized with HiveShard yet");
            return Task.Run(() =>
            {
                Interlocked.Increment(ref _ready);
                foreach (var tick in _ticks.GetConsumingEnumerable())
                {
                    if(tick.Message.TickNumber <= _lastTick)
                        continue;
                    _lastTick = tick.Message.TickNumber;
                    _loggingProvider.LogDebug($"Tick: {tick.Message.TickNumber}");
                    _tickRepository.SetLatestTick(tick.Message.TickNumber);
                    var offsets = tick.Message.ChunkOffsets
                        .ToDictionary(x => new TopicPartition(x.Topic, x.Partition), x => x.Offset);
                    foreach (var key in _events.Keys)
                        if (!offsets.ContainsKey(key))
                            offsets[key] = 0;

                    foreach (var eventTopic in _events)
                    {
                        _loggingProvider.LogDebug($"Handle {eventTopic.Key.Topic}:{eventTopic.Key.Chunk.Topic} events");
                        try
                        {
                            while (_eventQueueOffsets[eventTopic.Key] < offsets[eventTopic.Key])
                            {
                                var caster = eventTopic.Value.Take();
                                caster.Handler(caster.Consumption);
                                _eventQueueOffsets[eventTopic.Key] += 1;
                            }
                        }
                        catch (Exception e)
                        {
                            _loggingProvider.LogError(e, new LogOrigin(_hiveShardIdentity));
                            throw;
                        }
                    }
                    
                    _hiveShard.Process();
                    var groupedOffsets = _eventQueueOffsets.GroupBy(x=> x.Key.Topic,
                        x => new TopicPartitionOffset(x.Key.Topic, x.Key.Chunk, x.Value));
                    foreach (var groupedOffset in groupedOffsets)
                    {
                        var completedTick = CompletedTick.From(groupedOffset.Key, _hiveShardIdentity, tick.Message.TickNumber, groupedOffset.AsEnumerable());
                        _simpleFabric.Send<CompletedTick>("completed-ticks", new Partition(_eventRepository.GetEventOrder(groupedOffset.Key)), completedTick);
                    }
                }
            });
        }

        public Task Register<TEvent>(Action<TEvent> handler) where TEvent: IEvent
        {
            var fullName = typeof(TEvent).FullName;
            if (fullName is null)
                throw new Exception("event name was null");
        
            foreach (var neighbour in _hiveShardIdentity.Chunk.GetNeighboursAndSelf(_globalChunkConfig))
            {
                var topicPartition = new TopicPartition(fullName, neighbour);
                _eventQueueOffsets[topicPartition] = 0;
                _events[topicPartition] = new BlockingCollection<Caster>(50);
                _simpleFabric.Register<TEvent>(topicPartition.Topic, topicPartition.Chunk, x =>
                {
                    var consumption = new Consumption<object>(x.Message, x.Offset);
                    _events[topicPartition].Add(new Caster(consumption, o =>
                    {
                        var wrappedValue = (Consumption<object>)o;
                        handler((TEvent)wrappedValue.Message);
                    }));
                });
            }
            return Task.CompletedTask;
        }

        public Task Send<TEvent>(TEvent message) where TEvent: IEvent
        {
            return Resilience.Retry(_ =>
            {
                if (_ready < 1)
                    throw new Exception($"{nameof(ScopedShardTunnel)} is not ready yet");
                return _simpleFabric.Send(typeof(TEvent).FullName!, _hiveShardIdentity.Chunk, message);
            }, 
            $"{nameof(ScopedShardTunnel)} from {_hiveShardIdentity}", _cancellationProvider.GetToken(), _loggingProvider);

        }

        public async Task WaitForReady()
        {
            while (_ready < 1)
            {
                await Task.Delay(100);
            }
        }
    }
}