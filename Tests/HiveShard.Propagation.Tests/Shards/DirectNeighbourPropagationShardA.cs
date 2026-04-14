using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Propagation.Tests.Events;
using HiveShard.Shard.Interfaces;

namespace HiveShard.Propagation.Tests.Shards;

public class DirectNeighbourPropagationShardA: IHiveShard
{
    private Chunk? _chunk;
    private IScopedShardTunnel _tunnel;
    public int ReceivedSecret { get; private set; }

    public DirectNeighbourPropagationShardA(IScopedShardTunnel tunnel)
    {
        _tunnel = tunnel;
    }

    public void Initialize(Chunk chunk)
    {
        _chunk = chunk;
        _tunnel.Register<InitializingEvent>(HandleInitializingEvent);
    }

    private void HandleInitializingEvent(Message<InitializingEvent> message)
    {
        ReceivedSecret = message.Payload.Secret;
        _tunnel.Send(new DummyEventA());
    }
}