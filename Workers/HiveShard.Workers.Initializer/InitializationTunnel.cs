using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;
using HiveShard.Interface.Repository;

namespace HiveShard.Workers.Initializer;

public class InitializationTunnel: IInitializationTunnel
{
    private readonly ISimpleFabric _simpleFabric;
    private readonly IEventRepository _eventRepository;
    private readonly Dictionary<(Type, Chunk), long> _offsets = new();
    private InitializerType _emitterIdentity;
    private IInitializer _initializerInstance;

    public InitializationTunnel(ISimpleFabric simpleFabric, IEventRepository eventRepository)
    {
        _simpleFabric = simpleFabric;
        _eventRepository = eventRepository;
    }

    public void Send<TEvent>(TEvent e, Chunk chunk) where TEvent : IEvent
    {
        var index = (typeof(TEvent), chunk);

        if (!_offsets.ContainsKey(index))
            _offsets[index] = 0;
        _offsets[index]++;
        _simpleFabric.Send(index.Item1.FullName!, chunk, new Envelope<TEvent>(e, Guid.NewGuid()));
    }

    public void Initialize(IInitializer initializerInstance)
    {
        _initializerInstance = initializerInstance;
        var newGuid = Guid.NewGuid();
        _emitterIdentity = new InitializerType(new EmitterIdentity($"initializer-{newGuid}"));
        _simpleFabric.Register<Tick>("ticks", HandleTick);
    }

    private void HandleTick(Consumption<IEnvelope<Tick>> tickConsumption)
    {
        if (tickConsumption.Message.Payload.TickNumber != 2)
            return;
        
        foreach (var offsetsPerTopic in _offsets
                     .GroupBy(x=>x.Key.Item1, 
                         x => (x.Key.Item2, x.Value)))
        {
            List<TopicPartitionOffset> offsets = new();
            foreach (var (chunk, offset) in offsetsPerTopic)
            {
                offsets.Add(new TopicPartitionOffset(offsetsPerTopic.Key.FullName!, chunk, offset));
            }
            _simpleFabric.Send("completed-ticks",
                new Envelope<CompletedTick>(
                    CompletedTick.From(offsetsPerTopic.Key, _emitterIdentity, 0, offsets.AsEnumerable()),
                    Guid.NewGuid()
                )
            );
        }
    }
}