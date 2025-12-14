using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Workers.Shard.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Workers.Shard.Builder;

public class ShardWorkerBuilder
{
    private List<HiveShardIdentity> _hiveShards = new();
    private string _identifier = Guid.NewGuid().ToString();
    
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

    public ShardWorkerBuilder Identify(string identifier)
    {
        _identifier = identifier;
        return this;
    }
    
    internal ShardWorkerIsolatedEnvironment Build()
    {
        return new ShardWorkerIsolatedEnvironment(_identifier, _hiveShards.AsEnumerable());
    }
}