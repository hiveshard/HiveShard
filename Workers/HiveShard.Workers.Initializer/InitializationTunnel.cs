using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;
using HiveShard.Interface.Repository;
using HiveShard.Workers.Initializer.Data;

namespace HiveShard.Workers.Initializer;

public class InitializationTunnel: IInitializationTunnel
{
    private readonly ISimpleFabric _simpleFabric;
    private readonly IEventRepository _eventRepository;
    private readonly Dictionary<(string, Chunk), long> _offsets = new();
    private readonly Dictionary<string, long> _tickAdvances = new();
    private InitializerEmitterIdentity _emitterIdentity;
    private IInitializer _initializerInstance;
    private GlobalChunkConfig _globalChunkConfig;


    public InitializationTunnel(ISimpleFabric simpleFabric, IEventRepository eventRepository, GlobalChunkConfig globalChunkConfig)
    {
        _simpleFabric = simpleFabric;
        _eventRepository = eventRepository;
        _globalChunkConfig = globalChunkConfig;
    }

    public void Send<TEvent>(TEvent e, Chunk chunk) where TEvent : IEvent
    {
        var index = (typeof(TEvent).FullName!, chunk);

        if (!_offsets.ContainsKey(index))
            _offsets[index] = 0;
        _offsets[index]++;
        _simpleFabric.Send(index.Item1, chunk, new Envelope<TEvent>(e, Guid.NewGuid()));
    }

    public void Initialize(IInitializer initializerInstance, InitializerEmitterIdentity identity)
    {
        _initializerInstance = initializerInstance;
        _emitterIdentity = identity;
        foreach (var topic in _eventRepository.GetTopicsOfEmitter(identity.Identity))
        {
            _simpleFabric.Register<Tick>("ticks", new Partition(_eventRepository.GetEventOrder(topic)), HandleTick);
        }
    }

    private void HandleTick(Consumption<IEnvelope<Tick>> tickConsumption)
    {
        if (tickConsumption.Message.Payload.TickNumber == 0)
        {
            if(!TryAdvance(tickConsumption.Message.Payload))
                return;
            foreach (var topic in _eventRepository.GetTopicsOfEmitter(_emitterIdentity.Identity))
            {
                _simpleFabric.Send("completed-ticks", new Partition(_eventRepository.GetEventOrder(topic)),
                    new Envelope<CompletedTick>(
                        CompletedTick.From(topic, _emitterIdentity, 0, []),
                        Guid.NewGuid()
                    )
                );
            }
        }
        else if (tickConsumption.Message.Payload.TickNumber == 1)
        {
            if(!TryAdvance(tickConsumption.Message.Payload))
                return;
            _initializerInstance.Initialize(this);
            foreach (var topic in _eventRepository.GetTopicsOfEmitter(_emitterIdentity.Identity))
            {
                for (int x = _globalChunkConfig.MinChunk.XCoord; x <= _globalChunkConfig.MaxChunk.XCoord; x++)
                {
                    for (int y = _globalChunkConfig.MinChunk.XCoord; y <= _globalChunkConfig.MaxChunk.XCoord; y++)
                    {
                        var chunk = new Chunk(x,y);
                        if (!_offsets.TryGetValue((topic, chunk), out var offset))
                            throw new Exception(
                                $"Initializer {_initializerInstance.GetType().FullName!} did not initialize {topic}[chunk: {chunk}]");
                        
                        _simpleFabric.Send("completed-ticks", new Partition(_eventRepository.GetEventOrder(topic)),
                            new Envelope<CompletedTick>(
                                CompletedTick.From(topic, _emitterIdentity, 0, 
                                [
                                    new TopicPartitionOffset(topic, chunk, offset)
                                ]),
                                Guid.NewGuid()
                            )
                        );
                    }
                }
            }
        }
    }

    private bool TryAdvance(Tick tick)
    {
        if (!_tickAdvances.ContainsKey(tick.TickEventType))
            _tickAdvances[tick.TickEventType] = -1;
        if (_tickAdvances[tick.TickEventType] >= tick.TickNumber)
            return false;
        _tickAdvances[tick.TickEventType] = tick.TickNumber;

        foreach (var topic in _eventRepository.GetTopicsOfEmitter(_emitterIdentity.Identity))
        {
            if (_tickAdvances[topic] < tick.TickNumber)
                return false;
        }

        return true;
    }
}