using System.Threading;
using System.Threading.Tasks;
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
    private readonly TickerRepository _tickerRepository;
    private readonly TickerAdditionRepository _tickerAdditionRepository;
    private readonly ICancellationProvider _cancellationProvider;
    private readonly ISimpleFabric _simpleFabric;
    private readonly IEventRepository _eventRepository;

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

    private readonly TaskCompletionSource<bool> _completion = new();
    private int _activeTasks;

    public async Task Start()
    {
        while (!_cancellationProvider.GetToken().IsCancellationRequested)
        {
            while (_tickerAdditionRepository.TryConsumeEventTickerRequest(out DistributedTickerIdentity identity))
            {
                var tickerEmitterType = new TickerEmitterType(identity.ToEmitterIdentity());
                var eventTicker = new DistributedTicker(
                    new DistributedTickerConfig(identity.EventType, tickerEmitterType),
                    _simpleFabric,
                    _eventRepository);

                eventTicker.Initialize();
                _tickerRepository.AddTicker(identity.EventType, eventTicker);
            }

            while (_tickerAdditionRepository.TryConsumeGlobalTickerRequest(out GlobalTickerIdentity id))
            {
                var globalTicker = new GlobalTicker(id, _simpleFabric, _eventRepository);

                globalTicker.Initialize();
                _tickerRepository.AddGlobalTicker(id, globalTicker);
            }

            await Task.Delay(100);
        }
    }

}