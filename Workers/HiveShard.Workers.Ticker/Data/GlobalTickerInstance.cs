using System.Threading.Tasks;
using HiveShard.Ticker;

namespace HiveShard.Workers.Ticker.Data;

public class GlobalTickerInstance
{
    public GlobalTickerInstance(GlobalTicker globalTicker, Task task)
    {
        GlobalTicker = globalTicker;
        Task = task;
    }

    public GlobalTicker GlobalTicker { get; }
    public Task Task { get; }
}