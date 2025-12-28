using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Fabric;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
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
    private IWorkerLoggingProvider _workerLoggingProvider;
    private IShardRepository _shardRepository;
    private ServiceEnvironment _serviceEnvironment;

    public TickerWorker(
        TickerRepository tickerRepository, 
        TickerAdditionRepository tickerAdditionRepository, 
        ICancellationProvider cancellationProvider,
        ISimpleFabric simpleFabric,
        IWorkerLoggingProvider workerLoggingProvider,
        IShardRepository shardRepository, ServiceEnvironment serviceEnvironment)
    {
        _shardRepository = shardRepository;
        _serviceEnvironment = serviceEnvironment;
        _workerLoggingProvider = workerLoggingProvider;
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
                var eventTicker = new EventTicker(_simpleFabric, new TickerConfig(eventType), _workerLoggingProvider, _shardRepository);
                var task = Task.Run(() => eventTicker.Start());
                _tickerRepository.AddTicker(eventType, new EventTickerInstance(eventTicker, task));
            }
            
            
            await Task.Delay(100);
        }
    }
}