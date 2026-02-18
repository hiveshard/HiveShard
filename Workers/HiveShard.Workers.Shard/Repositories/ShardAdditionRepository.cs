using System.Collections.Concurrent;
using HiveShard.Workers.Shard.Data;

namespace HiveShard.Workers.Shard.Repositories;

public class ShardAdditionRepository
{
    private readonly ConcurrentQueue<ShardAdditionRequest> _requests = new();

    public void Add(ShardAdditionRequest shardAdditionRequest)
    {
        _requests.Enqueue(shardAdditionRequest);
    }

    public bool TryRetrieve(out ShardAdditionRequest shardAdditionRequest)
    {
        return _requests.TryDequeue(out shardAdditionRequest);
    }
}