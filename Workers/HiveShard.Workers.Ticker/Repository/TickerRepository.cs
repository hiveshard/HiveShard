using HiveShard.Interface;
using HiveShard.Ticker;

namespace HiveShard.Workers.Ticker.Repository;

public class TickerRepository
{
    private readonly Dictionary<Type, EventTicker> _map = new();

    public void AddTicker(Type eventType, EventTicker eventTicker)
    {
        _map.Add(eventType, eventTicker);
    }

    public EventTicker GetTicker(Type eventType)
    {
        return _map[eventType];
    }

    public IEnumerable<EventTicker> GetAll() => _map.Values;
}