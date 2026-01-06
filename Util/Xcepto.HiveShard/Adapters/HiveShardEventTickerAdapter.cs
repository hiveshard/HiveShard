using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Interface;
using HiveShard.Interface.Repository;
using HiveShard.Ticker.Data;
using Xcepto.Adapters;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters;

public class HiveShardEventTickerAdapter: XceptoAdapter
{
    private IEventRepository _eventRepository;
    private DistributedTickerIdentity _tickerIdentity;

    public HiveShardEventTickerAdapter(DistributedTickerIdentity tickerIdentity, IEventRepository eventRepository)
    {
        _tickerIdentity = tickerIdentity;
        _eventRepository = eventRepository;
    }
    public void ExpectTick(long tickNumber)
    {
        var emitterIdentity = _tickerIdentity.ToEmitterIdentity();
        var partition = new Partition(_eventRepository.GetEventOrder(_tickerIdentity.EventType));
        AddStep(new TickerTickExpectationState($"Expect tick {tickNumber} on partition {partition.Value} from {emitterIdentity.EmitterIdentityString}", 
            partition,
            tick => tick.TickNumber.Equals(tickNumber) 
                    && tick.Emitter.Equals(emitterIdentity)
                    && tick.TickEventType.Equals(typeof(Tick).FullName!)));
    }
    
}