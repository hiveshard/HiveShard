using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Shard.Interfaces;
using HiveShard.Xcepto.Tests.Event;

namespace HiveShard.Xcepto.Tests.Shards;

public class StateShard: IHiveShard
{
    public StateShard(IScopedShardTunnel tunnel)
    {
        _tunnel = tunnel;
    }

    public Chunk? Chunk { get; private set; }
    public bool Initialized { get; private set; }
    
    private IScopedShardTunnel _tunnel;

    public void Initialize(Chunk chunk)
    {
        Chunk = chunk;
        _tunnel.Register<InitializationEvent>(HandleAllInitializationEvent);
    }

    private void HandleAllInitializationEvent(Message<InitializationEvent> message)
    {
        if(!message.Payload.Target.Equals(Chunk))
            return;

        Initialized = true;
        _tunnel.Send(new DummyEvent());
    }
}