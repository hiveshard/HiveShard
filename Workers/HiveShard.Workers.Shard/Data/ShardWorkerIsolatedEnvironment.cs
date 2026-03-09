using System;
using System.Collections.Generic;
using HiveShard.Data;

namespace HiveShard.Workers.Shard.Data;

public class ShardWorkerIsolatedEnvironment: IsolatedEnvironment
{
    public IEnumerable<HiveShardIdentity> HiveShards { get; }

    public ShardWorkerIsolatedEnvironment(Guid identifier, IEnumerable<HiveShardIdentity> hiveShards)
    {
        HiveShards = hiveShards;
        Identifier = identifier;
    }

    public Guid Identifier { get; }
    public override bool IsUnique => false;
}