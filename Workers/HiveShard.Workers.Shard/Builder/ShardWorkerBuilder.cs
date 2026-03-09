using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Workers.Shard.Data;

namespace HiveShard.Workers.Shard.Builder;

public class ShardWorkerBuilder
{
    private readonly List<HiveShardIdentity> _hiveShards = new();
    private Guid _identifier = Guid.NewGuid();
    
    public ShardWorkerBuilder AddShard<T>(Chunk chunk, Guid identity)
        where T : class, IHiveShard
    {
        return AddShard(new HiveShardIdentity(chunk, ShardType.From<T>(), identity));
    }

    
    public ShardWorkerBuilder AddShard(HiveShardIdentity shardIdentity)
    {
        _hiveShards.Add(shardIdentity);
        return this;
    }

    public ShardWorkerBuilder Identify(Guid id)
    {
        _identifier = id;
        return this;
    }
    
    internal ShardWorkerIsolatedEnvironment Build()
    {
        return new ShardWorkerIsolatedEnvironment(_identifier, _hiveShards.AsEnumerable());
    }
}