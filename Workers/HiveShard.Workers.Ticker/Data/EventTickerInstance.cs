using System.Threading.Tasks;
using HiveShard.Ticker;

namespace HiveShard.Workers.Ticker.Data;

public class EventTickerInstance
{
    public EventTickerInstance(DistributedTicker instance, Task task)
    {
        Instance = instance;
        Task = task;
    }

    public DistributedTicker Instance { get; }
    public Task Task { get; }
}