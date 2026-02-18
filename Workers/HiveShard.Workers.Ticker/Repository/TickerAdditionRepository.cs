using System.Collections.Concurrent;
using HiveShard.Ticker.Data;

namespace HiveShard.Workers.Ticker.Repository;

public class TickerAdditionRepository
{
    private readonly ConcurrentQueue<DistributedTickerIdentity> _eventTickersToBeAdded = new();
    private readonly ConcurrentQueue<GlobalTickerIdentity> _globalTickersToBeAdded = new();

    public void RequestEventTickerAddition(DistributedTickerIdentity type)
    {
        _eventTickersToBeAdded.Enqueue(type);
    }

    public void RequestGlobalTickerAddition(GlobalTickerIdentity globalTickerIdentity)
    {
        _globalTickersToBeAdded.Enqueue(globalTickerIdentity);
    }

    public bool TryConsumeEventTickerRequest(out DistributedTickerIdentity eventType)
    {
        return _eventTickersToBeAdded.TryDequeue(out eventType);
    }
    
    public bool TryConsumeGlobalTickerRequest(out GlobalTickerIdentity globalTickerIdentity)
    {
        return _globalTickersToBeAdded.TryDequeue(out globalTickerIdentity);
    }
}