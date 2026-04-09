using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Interface.Repository;
using HiveShard.Workers.Ticker.Data;

namespace HiveShard.Workers.Ticker;

public class GlobalTicker
{
    private readonly GlobalTickerIdentity _globalTickerIdentity;
    private readonly ISimpleFabric _simpleFabric;
    private readonly IEventRepository _eventRepository;

    public GlobalTicker(GlobalTickerIdentity globalTickerIdentity, ISimpleFabric simpleFabric, IEventRepository eventRepository)
    {
        _globalTickerIdentity = globalTickerIdentity;
        _simpleFabric = simpleFabric;
        _eventRepository = eventRepository;
    }

    private long _currentTick;
    public void Initialize()
    {
        var tickEventName = typeof(Tick).FullName!;
        // this tick should be ignored if receivers already know of something > 0
        _simpleFabric.Send(typeof(Tick).FullName!, new Partition(0),
            new Envelope<Tick>(
                new Tick(0, [], DateTime.Now, tickEventName, _globalTickerIdentity.ToEmitterType()),
                Guid.NewGuid()
            )
        );
        _currentTick = 0;
        
        
        _simpleFabric.Register<CompletedTick>(typeof(CompletedTick).FullName!, new Partition(0), HandleEventCompletedTick);
    }


    private readonly Dictionary<long, ISet<EmitterIdentity>> _completedTicks = new();
    private void HandleEventCompletedTick(Consumption<IEnvelope<CompletedTick>> consumption)
    {
        var completedTick = consumption.Message.Payload;
        var messageTick = completedTick.Tick;
        if (messageTick < _currentTick) 
            return;
        
        if (!_completedTicks.ContainsKey(messageTick))
            _completedTicks[messageTick] = new HashSet<EmitterIdentity>();
        
        _completedTicks[messageTick].Add(completedTick.EmitterIdentity);

        foreach (var registeredElement in _eventRepository.GetTotalOrder())
        {
            // Ignore initialization ticks after tick 1
            if(messageTick > 1 && _eventRepository.GetInitializationOnlyEvents().Contains(registeredElement.Key))
                continue;
            
            foreach (var emitter in _eventRepository.GetEmitters(registeredElement.Key))
            {
                // require all remaining ticks
                if(!_completedTicks[messageTick].Contains(emitter.Identity))
                    return;   
            }
        }

        _currentTick += 1;
        _simpleFabric.Send(typeof(Tick).FullName!, new Partition(0),
            new Envelope<Tick>(
                new Tick(_currentTick, [], DateTime.Now, typeof(Tick).FullName!, _globalTickerIdentity.ToEmitterType()),
                Guid.NewGuid()
            )
        );
    }
}