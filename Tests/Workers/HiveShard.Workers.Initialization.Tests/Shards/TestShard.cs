using HiveShard.Interface;
using HiveShard.Shard.Interfaces;
using HiveShard.Workers.Initialization.Tests.Events;
using HiveShard.Workers.Initialization.Tests.Repositories;

namespace HiveShard.Workers.Initialization.Tests.Shards;

public class TestShard: IHiveShard
{
    private readonly TestRepository _testRepository = new();
    private readonly IScopedShardTunnel _scopedShardTunnel;

    public TestShard(IScopedShardTunnel scopedShardTunnel)
    {
        _scopedShardTunnel = scopedShardTunnel;
    }

    public int ReceivedIncrements { get; private set; } = 0;

    public void Process()
    {
        while (_testRepository.TryGet(out int increment)) ReceivedIncrements += increment;
    }

    public void Initialize()
    {
        _scopedShardTunnel.Register<InitialDataEvent>(x =>
        {
            _testRepository.AddInt(x.Value);
        });
    }
}