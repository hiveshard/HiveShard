using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Interface.Repository;
using HiveShard.Ticker.Data;

namespace HiveShard.Ticker;

public class GlobalTicker
{
    private GlobalTickerIdentity _globalTickerIdentity;
    private ISimpleFabric _simpleFabric;
    private IEventRepository _eventRepository;

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
        _simpleFabric.Send(tickEventName, new Tick(0, [], DateTime.Now, tickEventName));
        _currentTick = 0;
        
        
        foreach (var eventOrder in _eventRepository.GetTotalOrder())
        {
            _simpleFabric.Register<CompletedTick>("completed-ticks", new Partition(eventOrder.Value), HandleEventCompletedTick);
        }
        
        return Task.CompletedTask;
    }

    
    private void HandleEventCompletedTick(Consumption<CompletedTick> consumption)
    {
        if (consumption.Message.Tick < _currentTick) 
            return;
        
        
    }
}