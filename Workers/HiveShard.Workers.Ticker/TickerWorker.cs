using System;
using System.Threading.Tasks;
using HiveShard.Interface;
using HiveShard.Interface.Providers;
using HiveShard.Interface.Repository;
using HiveShard.Ticker;
using HiveShard.Workers.Ticker.Data;
using HiveShard.Workers.Ticker.Repository;

namespace HiveShard.Workers.Ticker;

public class TickerWorker: IIsolatedEntryPoint
{
    private TickerRepository _tickerRepository;
    private TickerAdditionRepository _tickerAdditionRepository;
    private ICancellationProvider _cancellationProvider;
    private ISimpleFabric _simpleFabric;
    private IEventRepository _eventRepository;

    public TickerWorker(
        TickerRepository tickerRepository, 
        TickerAdditionRepository tickerAdditionRepository, 
        ICancellationProvider cancellationProvider,
        ISimpleFabric simpleFabric, IEventRepository eventRepository)
    {
        _simpleFabric = simpleFabric;
        _eventRepository = eventRepository;
        _tickerRepository = tickerRepository;
        _tickerAdditionRepository = tickerAdditionRepository;
        _cancellationProvider = cancellationProvider;
    }

    public async Task Start()
    {
        while (!_cancellationProvider.GetToken().IsCancellationRequested)
        {
            while (_tickerAdditionRepository.TryConsumeRequest(out Type eventType))
            {
                var eventTicker = new DistributedTicker(new DistributedTickerConfig(eventType), _simpleFabric, _eventRepository);
                var task = Task.Run(() => eventTicker.Start());
                _tickerRepository.AddTicker(eventType, new EventTickerInstance(eventTicker, task));
            }
            
            
            await Task.Delay(100);
        }
    }
}