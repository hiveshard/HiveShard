using HiveShard.Interface.Config;

namespace HiveShard.Interface.Logging
{
    public interface IFabricLoggingProvider
    {
        public IScopedFabricLoggingProvider GetScopedLogger<T>(IIdentityConfig identityConfig);
        
    }
}