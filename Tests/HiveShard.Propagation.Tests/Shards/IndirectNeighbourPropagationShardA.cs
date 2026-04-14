using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Propagation.Tests.Events;
using HiveShard.Shard.Interfaces;

namespace HiveShard.Propagation.Tests.Shards;

public class IndirectNeighbourPropagationShardA: IHiveShard
{
    private Chunk? _chunk;
    private IScopedShardTunnel _tunnel;
    public int ReceivedSecret { get; private set; }

    public IndirectNeighbourPropagationShardA(IScopedShardTunnel tunnel)
    {
        _tunnel = tunnel;
    }

    public void Initialize(Chunk chunk)
    {
        _chunk = chunk;
        _tunnel.Register<InitializingEvent>(HandleInitializingEvent);
        _tunnel.Register<TransitioningEvent>(HandleTransitioningEvent);
    }
    
    private void HandleTransitioningEvent(Message<TransitioningEvent> message)
    {
        // already received information
        if(ReceivedSecret == message.Payload.Secret)
            return;

        ReceivedSecret = message.Payload.Secret;
        _tunnel.Send(new TransitioningEvent(message.Payload.Secret));
    }

    private void HandleInitializingEvent(Message<InitializingEvent> message)
    {
        if(!message.Chunk.Equals(_chunk))
            return;
        
        _tunnel.Send(new TransitioningEvent(message.Payload.Secret));
    }
}