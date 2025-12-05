using System.Collections.Concurrent;

namespace HiveShard.Workers.Ticker.Repository;

public class TickerAdditionRepository
{
    private ConcurrentQueue<Type> _tickersToBeAdded = new();

    public void RequestAddition(Type type)
    {
        _tickersToBeAdded.Enqueue(type);
    }

    public bool TryConsumeRequest(out Type eventType)
    {
        return _tickersToBeAdded.TryDequeue(out eventType);
    }
}