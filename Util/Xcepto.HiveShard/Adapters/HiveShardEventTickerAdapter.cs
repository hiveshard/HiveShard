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
    private EmitterIdentity _emitterIdentity;
    private Partition _partition;

    public HiveShardEventTickerAdapter(DistributedTickerIdentity tickerIdentity, IEventRepository eventRepository)
    {
        _tickerIdentity = tickerIdentity;
        _eventRepository = eventRepository;
        _emitterIdentity = _tickerIdentity.ToEmitterIdentity();
        _partition = new Partition(_eventRepository.GetEventOrder(_tickerIdentity.EventType));
    }
    public void ExpectTick(long tickNumber)
    {
        AddStep(new TickerExpectationState<Tick>(
            $"Expect tick {tickNumber} on partition {_partition.Value} from {_emitterIdentity.EmitterIdentityString}", 
            "ticks", _partition,
            tick => tick.TickNumber.Equals(tickNumber) 
                    && tick.Emitter.Equals(_emitterIdentity)
                    && tick.TickEventType.Equals(_tickerIdentity.EventType.FullName!)));
    }
    public void ExpectCompletedTick(long tickNumber)
    {
        AddStep(new TickerExpectationState<CompletedTick>(
            $"Expect tick {tickNumber} on partition {_partition.Value} from {_emitterIdentity.EmitterIdentityString}", 
            "completed-ticks", _partition,
            tick => tick.Tick.Equals(tickNumber) 
                    && tick.EmitterIdentity.Equals(_emitterIdentity)
                    && tick.EventType.Equals(_tickerIdentity.EventType.FullName!)));
    }
    
}