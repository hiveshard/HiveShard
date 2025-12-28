using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabric;
using HiveShard.Interface.Repository;

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
        _simpleFabric.Register<CompletedTick>("completed-ticks", eventOrder,
            consumption =>
            {
                
            });
        return Task.CompletedTask;
    }
}