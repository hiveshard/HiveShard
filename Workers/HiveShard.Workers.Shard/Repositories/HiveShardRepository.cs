using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Workers.Shard.Repositories;

public class HiveShardRepository
{
    private static int _nextId;
    public int InstanceId { get; } = Interlocked.Increment(ref _nextId);
    
    private readonly ConcurrentDictionary<HiveShardIdentity, IServiceProvider> _shards = new();

    public void AddHiveShard(HiveShardIdentity identity, IServiceProvider serviceProvider)
    {
        if (!_shards.TryAdd(identity, serviceProvider))
            throw new Exception("Hiveshard already existed");
    }

    public bool TryGetHiveShard(HiveShardIdentity identity, out IServiceProvider serviceProvider)
    {
        return _shards.TryGetValue(identity, out serviceProvider);
    }
}