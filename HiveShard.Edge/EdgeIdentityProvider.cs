using System;
using HiveShard.Interface;
using HiveShard.Interface.Config;

namespace HiveShard.Edge
{
    public class EdgeIdentityProvider: IAddressProvider
    {
        private IIdentityConfig _identityConfig;

        public EdgeIdentityProvider(IIdentityConfig identityConfig)
        {
            _identityConfig = identityConfig;
        }

        public Uri GetUri() => new Uri($"https://{_identityConfig.GetIdentity()}.edge.magetown.local");
    }
}