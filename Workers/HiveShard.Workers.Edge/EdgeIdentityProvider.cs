using System;
using HiveShard.Interface.Config;
using HiveShard.Interface.Providers;

namespace HiveShard.Workers.Edge;

public class EdgeIdentityProvider: IAddressProvider
{
    private readonly IIdentityConfig _identityConfig;

    public EdgeIdentityProvider(IIdentityConfig identityConfig)
    {
        _identityConfig = identityConfig;
    }

    public Uri GetUri() => new($"https://{_identityConfig.GetIdentity()}.edge.magetown.local");
}