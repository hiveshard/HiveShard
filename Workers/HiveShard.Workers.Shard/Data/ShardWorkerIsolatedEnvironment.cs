using System;
using System.Collections.Generic;
using HiveShard.Data;

namespace HiveShard.Workers.Shard.Data;

public class ShardWorkerIsolatedEnvironment: IsolatedEnvironment
{
    public IEnumerable<HiveShardIdentity> HiveShards { get; }

    public ShardWorkerIsolatedEnvironment(string identifier, IEnumerable<HiveShardIdentity> hiveShards)
    {
        HiveShards = hiveShards;
        Identifier = identifier;
    }

    public string Identifier { get; }
}