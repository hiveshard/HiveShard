using System;
using System.Collections.Concurrent;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Interface.Repository;
using HiveShard.Workers.Ticker.Util;

namespace HiveShard.Workers.Ticker;

public class DistributedTicker
{
    private readonly DistributedTickerConfig _config;
    private readonly ISimpleFabric _simpleFabric;
    private readonly IEventRepository _eventRepository;
    public DistributedTicker(DistributedTickerConfig config, ISimpleFabric simpleFabric, IEventRepository eventRepository)
    {
        _config = config;
        _simpleFabric = simpleFabric;
        _eventRepository = eventRepository;
    }

    public void Initialize()
    {
        var eventOrder = new Partition(_eventRepository.GetEventOrder(_config.EventType));
        _simpleFabric.Register<Tick>(typeof(Tick).FullName!, new Partition(0), HandleGlobalTicks);
        _simpleFabric.Register<CompletedTick>(typeof(CompletedTick).FullName!, eventOrder,
            HandleEventSpecificCompletedTick);
    }

    private readonly ConcurrentDictionary<long, ConcurrentHashSet<EmitterIdentity>> _completedEventTicks = new();

    private readonly bool _currentRoundOngoing = false;
    
    // initialized without tick
    private long _currentTick = -1;
    
    // no tick allowed at first
    private long _allowanceTick = -1;
    private void HandleGlobalTicks(Consumption<IEnvelope<Tick>> consumption)
    {
        var messageTick = consumption.Message.Payload.TickNumber;
        if(messageTick < _currentTick)
            return;

        if (messageTick > _allowanceTick)
            _allowanceTick = messageTick;

        
        if(messageTick == _currentTick + 1 && !_currentRoundOngoing)
        {
            _currentTick += 1;
            var eventPartition = new Partition(_eventRepository.GetEventOrder(_config.EventType));
            _simpleFabric.Send(typeof(Tick).FullName!, eventPartition,
                new Envelope<Tick>(
                    new Tick(_currentTick, [], DateTime.Now, _config.EventType.FullName!, _config.EmitterType.Identity),
                    Guid.NewGuid())
                );
        }
            
    }

    private void HandleEventSpecificCompletedTick(Consumption<IEnvelope<CompletedTick>> consumption)
    {
        var completedTick = consumption.Message.Payload;
        if (completedTick.Tick > _currentTick)
            throw new NotImplementedException("We did not start this round of ticks, no ticker redundancy yet");
        
        var hashSet = _completedEventTicks.GetOrAdd(completedTick.Tick, 
            _ => new ConcurrentHashSet<EmitterIdentity>());
        
        // skip duplicates
        if (hashSet.Contains(completedTick.EmitterIdentity))
            return;
        
        hashSet.Add(completedTick.EmitterIdentity);

        foreach (var eventEmitterType in _eventRepository.GetEmitters(_config.EventType.FullName!))
        {
            // not done yet
            if (!hashSet.Contains(eventEmitterType.Identity))
                return;
        }

        FinalizeTick();
    }

    private void FinalizeTick()
    {
        _simpleFabric.Send(typeof(CompletedTick).FullName!, new Partition(0),
            new Envelope<CompletedTick>(
                new CompletedTick(_config.EmitterType.Identity, _currentTick, _config.EventType.FullName!, []),
                Guid.NewGuid()
            )
        );
    }


}