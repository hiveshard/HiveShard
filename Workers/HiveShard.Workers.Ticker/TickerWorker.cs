using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Providers;
using HiveShard.Interface.Repository;
using HiveShard.Ticker;
using HiveShard.Ticker.Data;
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
            while (_tickerAdditionRepository.TryConsumeEventTickerRequest(out Type eventType))
            {
                var tickerEmitterType = new TickerEmitterType(new EmitterIdentity($"{eventType.FullName!}"));
                var eventTicker = new DistributedTicker(new DistributedTickerConfig(eventType, tickerEmitterType), _simpleFabric, _eventRepository);
                var task = Task.Run(() => eventTicker.Start());
                _tickerRepository.AddTicker(eventType, new EventTickerInstance(eventTicker, task));
            }
            
            while (_tickerAdditionRepository.TryConsumeGlobalTickerRequest(out GlobalTickerIdentity globalTickerIdentity))
            {
                var globalTicker = new GlobalTicker(globalTickerIdentity, _simpleFabric, _eventRepository);
                var task = Task.Run(() => globalTicker.Start());
                _tickerRepository.AddGlobalTicker(globalTickerIdentity, new GlobalTickerInstance(globalTicker, task));
            }
            
            
            await Task.Delay(100);
        }
    }
}