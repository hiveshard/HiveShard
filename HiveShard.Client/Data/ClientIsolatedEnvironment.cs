using System;
using HiveShard.Data;

namespace HiveShard.Client.Data;

public class ClientIsolatedEnvironment: IsolatedEnvironment
{
    internal ClientIsolatedEnvironment(HiveShardClient user)
    {
        User = user;
    }

    public HiveShardClient User { get; }
    public override bool IsUnique => false;
}