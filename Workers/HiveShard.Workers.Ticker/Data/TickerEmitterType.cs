using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Workers.Ticker.Data;

public class TickerEmitterType: IEventEmitterType
{
    public TickerEmitterType(EmitterIdentity identity)
    {
        Identity = identity;
    }

    public EmitterIdentity Identity { get; }
    public bool InitializationTickOnly => false;
}