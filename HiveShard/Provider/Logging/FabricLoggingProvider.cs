using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;

namespace HiveShard.Provider.Logging
{
    public class FabricLoggingProvider: IFabricLoggingProvider
    {
        private ITelemetryProvider _telemetryProvider;
        private ITickRepository _tickRepository;

        public FabricLoggingProvider(ITelemetryProvider telemetryProvider, ITickRepository tickRepository)
        {
            _telemetryProvider = telemetryProvider;
            _tickRepository = tickRepository;
        }

        public IScopedFabricLoggingProvider GetScopedLogger<T>(IIdentityConfig identityConfig)
        {
            var loggingProvider = new ScopedFabricLoggingProvider(_telemetryProvider, identityConfig, typeof(T), _tickRepository);
            return loggingProvider;
        }
    }
}