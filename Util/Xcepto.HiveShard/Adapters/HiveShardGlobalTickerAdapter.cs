using HiveShard.Data;
using HiveShard.Event;
using Xcepto.Adapters;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters;

public class HiveShardGlobalTickerAdapter: XceptoAdapter
{
    private readonly EmitterIdentity _emitterIdentity;
    public HiveShardGlobalTickerAdapter(EmitterIdentity emitterIdentity)
    {
        _emitterIdentity = emitterIdentity;
    }
    public void ExpectTick(long tickNumber)
    {
        AddStep(new TickerExpectationState<Tick>(
            $"Expect tick {tickNumber} on partition {0} from {_emitterIdentity.EmitterIdentityString}", 
            typeof(Tick).FullName!, new Partition(0), _emitterIdentity,
            tick => tick.TickNumber.Equals(tickNumber) && tick.Emitter.Equals(_emitterIdentity)
                && tick.TickEventType.Equals(typeof(Tick).FullName!)));
    }
    
}