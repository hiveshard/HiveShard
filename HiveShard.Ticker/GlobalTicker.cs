using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Interface.Repository;
using HiveShard.Ticker.Data;

namespace HiveShard.Ticker;

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
    public Task Start()
    {
        var tickEventName = typeof(Tick).FullName!;
        // this tick should be ignored if receivers already know of something > 0
        _simpleFabric.Send("ticks", new Partition(0),
            new Envelope<Tick>(
                new Tick(0, [], DateTime.Now, tickEventName, _globalTickerIdentity.ToEmitterType()),
                Guid.NewGuid()
            )
        );
        _currentTick = 0;
        
        
        foreach (var eventOrder in _eventRepository.GetTotalOrder()) 
            _simpleFabric.Register<CompletedTick>("completed-ticks", new Partition(eventOrder.Value), HandleEventCompletedTick);

        return Task.CompletedTask;
    }


    private readonly Dictionary<long, ISet<string>> _completedTicks = new();
    private void HandleEventCompletedTick(Consumption<IEnvelope<CompletedTick>> consumption)
    {
        var completedTick = consumption.Message.Payload;
        var messageTick = completedTick.Tick;
        if (messageTick < _currentTick) 
            return;
        
        if (!_completedTicks.ContainsKey(messageTick))
            _completedTicks[messageTick] = new HashSet<string>();
        _completedTicks[messageTick].Add(completedTick.EventType);

        foreach (var eventTypeString in _eventRepository.GetTotalOrder()
                     .Select(x => x.Key))
        {
            // Ignore initialization ticks outside of tick 1
            if(messageTick != 1 && _eventRepository.GetInitializationOnlyEvents().Contains(eventTypeString))
                continue;
            
            // require all remaining ticks
            if(!_completedTicks[messageTick].Contains(eventTypeString))
                return;
        }

        _currentTick += 1;
        _simpleFabric.Send("ticks", new Partition(0),
            new Envelope<Tick>(
                new Tick(_currentTick, [], DateTime.Now, typeof(Tick).FullName!, _globalTickerIdentity.ToEmitterType()),
                Guid.NewGuid()
            )
        );
    }
}