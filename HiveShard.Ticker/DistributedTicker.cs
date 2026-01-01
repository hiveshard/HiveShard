using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Interface.Repository;
using HiveShard.Ticker.Util;

namespace HiveShard.Ticker;

public class DistributedTicker
{
    private DistributedTickerConfig _config;
    private ISimpleFabric _simpleFabric;
    private IEventRepository _eventRepository;
    public DistributedTicker(DistributedTickerConfig config, ISimpleFabric simpleFabric, IEventRepository eventRepository)
    {
        _config = config;
        _simpleFabric = simpleFabric;
        _eventRepository = eventRepository;
    }

    public Task Start()
    {
        var eventOrder = new Partition(_eventRepository.GetEventOrder(_config.EventType));
        _simpleFabric.Register<Tick>("ticks", new Partition(0), HandleGlobalTicks);
        _simpleFabric.Register<CompletedTick>("completed-ticks", eventOrder,
            HandleEventSpecificCompletedTick);
        return Task.CompletedTask;
    }

    private ConcurrentDictionary<long, ConcurrentHashSet<EmitterIdentity>> _completedEventTicks = new();

    private long _currentTick;
    private void HandleGlobalTicks(Consumption<Tick> consumption)
    {
        var messageTick = consumption.Message.TickNumber;
        if(messageTick < _currentTick)
            return;

        throw new NotImplementedException();
    }

    private void HandleEventSpecificCompletedTick(Consumption<CompletedTick> consumption)
    {
        var hashSet = _completedEventTicks.GetOrAdd(consumption.Message.Tick, 
            _ => new ConcurrentHashSet<EmitterIdentity>());
        
        // skip duplicates
        if (!hashSet.Contains(consumption.Message.EmitterIdentity))
            return;

        foreach (var eventEmitterType in _eventRepository.GetEmitters(_config.EventType.FullName!))
        {
            // not done yet
            if(!hashSet.Contains(eventEmitterType.Identity))
                return;
        }
        
        // finalize event scope tick
        _currentTick += 1;
        var thisTickersPartition = new Partition(_eventRepository.GetEventOrder(_config.EventType));
        _simpleFabric.Send("completed-ticks", thisTickersPartition,
            new CompletedTick(_config.EmitterType.Identity, _currentTick, _config.EventType.FullName!, []));
    }
    
}