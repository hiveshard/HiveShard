using HiveShard.Fabric.Ticker;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Ticker;
using HiveShard.Workers.Ticker.Repository;

namespace HiveShard.Workers.Ticker;

public class TickerWorker: IIsolatedEntryPoint
{
    private TickerRepository _tickerRepository;
    private TickerAdditionRepository _tickerAdditionRepository;
    private ICancellationProvider _cancellationProvider;
    private ISimpleFabric _simpleFabric;
    private TickerConfig _tickerConfig;
    private IWorkerLoggingProvider _workerLoggingProvider;
    private IShardRepository _shardRepository;

    public TickerWorker(
        TickerRepository tickerRepository, 
        TickerAdditionRepository tickerAdditionRepository, 
        ICancellationProvider cancellationProvider,
        ISimpleFabric simpleFabric,
        TickerConfig tickerConfig,
        IWorkerLoggingProvider workerLoggingProvider,
        IShardRepository shardRepository)
    {
        _shardRepository = shardRepository;
        _workerLoggingProvider = workerLoggingProvider;
        _tickerConfig = tickerConfig;
        _simpleFabric = simpleFabric;
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
                var eventTicker = new EventTicker(_simpleFabric, _tickerConfig, _workerLoggingProvider, _shardRepository);
                _tickerRepository.AddTicker(eventType, eventTicker);
            }
            
            
            await Task.Delay(100);
        }
    }
}