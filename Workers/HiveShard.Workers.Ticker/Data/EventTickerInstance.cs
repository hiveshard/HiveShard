using HiveShard.Ticker;

namespace HiveShard.Workers.Ticker.Data;

public class EventTickerInstance
{
    public EventTickerInstance(EventTicker instance, Task task)
    {
        Instance = instance;
        Task = task;
    }

    public EventTicker Instance { get; }
    public Task Task { get; }
}