using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Shard.Interfaces;
using HiveShard.Worker.Tests.Data;
using HiveShard.Worker.Tests.Events;

namespace HiveShard.Worker.Tests.Shards;

public class TestShard: IHiveShard
{
    private readonly IScopedShardTunnel _tunnel;
    public int Sum { get; private set; } = 0;

    public TestShard(IScopedShardTunnel tunnel)
    {
        _tunnel = tunnel;
    }

    public void Initialize(Chunk chunk)
    {
        _tunnel.Register<TestEvent1>(e =>
        {
            if (e.Chunk.Equals(chunk))
            {
                if (e.Payload.Operation == Operation.Addition)
                    Sum += e.Payload.Number;
                else if (e.Payload.Operation == Operation.Multiplication)
                    Sum *= e.Payload.Number;
            }
        });
        
        _tunnel.Register<TestEvent2>(e =>
        {
            if (e.Payload.Operation == Operation.Addition)
                Sum += e.Payload.Number;
            else if (e.Payload.Operation == Operation.Multiplication)
                Sum *= e.Payload.Number;
        });
    }
}