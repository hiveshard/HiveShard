using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    private readonly TaskCompletionSource<bool> _completion = new();
    private int _activeTasks;

    public Task Start()
    {
        StartTracked(Task.Run(ManageAdditions));

        foreach (var t in _tickerRepository.GetAll())
            StartTracked(t.Task);

        foreach (var t in _tickerRepository.GetGlobalTickers())
            StartTracked(t.Task);

        return _completion.Task;
    }

    private void StartTracked(Task task)
    {
        Interlocked.Increment(ref _activeTasks);

        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                var ex =
                    t.Exception!.InnerExceptions.Count == 1
                        ? t.Exception.InnerExceptions[0]
                        : t.Exception.GetBaseException();

                _completion.TrySetException(ex);
                return;
            }

            if (Interlocked.Decrement(ref _activeTasks) == 0)
                _completion.TrySetResult(true);

        }, TaskContinuationOptions.ExecuteSynchronously);
    }


    private async Task ManageAdditions()
    {
        while (!_cancellationProvider.GetToken().IsCancellationRequested)
        {
            while (_tickerAdditionRepository.TryConsumeEventTickerRequest(out Type eventType))
            {
                var tickerEmitterType = new TickerEmitterType(new EmitterIdentity(eventType.FullName!));
                var eventTicker = new DistributedTicker(
                    new DistributedTickerConfig(eventType, tickerEmitterType),
                    _simpleFabric,
                    _eventRepository);

                var task = Task.Run(eventTicker.Start);
                _tickerRepository.AddTicker(eventType, new EventTickerInstance(eventTicker, task));

                StartTracked(task); // ← THIS is the missing piece
            }

            while (_tickerAdditionRepository.TryConsumeGlobalTickerRequest(out GlobalTickerIdentity id))
            {
                var globalTicker = new GlobalTicker(id, _simpleFabric, _eventRepository);

                var task = Task.Run(globalTicker.Start);
                _tickerRepository.AddGlobalTicker(id, new GlobalTickerInstance(globalTicker, task));

                StartTracked(task); // ← AND HERE
            }

            await Task.Delay(100);
        }
    }

}