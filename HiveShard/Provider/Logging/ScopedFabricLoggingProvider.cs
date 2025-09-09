using System;
using HiveShard.Data;
using HiveShard.Data.Telemetry;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;

namespace HiveShard.Provider.Logging
{
    public class ScopedFabricLoggingProvider: IScopedFabricLoggingProvider
    {
        private ITelemetryProvider _telemetry;
        private IIdentityConfig _identityConfig;
        private ITickRepository _tickRepository;
        private Type _type;
        private FabricInstance _fabricInstance;

        public ScopedFabricLoggingProvider(ITelemetryProvider telemetry, IIdentityConfig identityConfig, Type type, ITickRepository tickRepository)
        {
            _type = type;
            _tickRepository = tickRepository;
            _identityConfig = identityConfig;
            _telemetry = telemetry;
            _fabricInstance = new FabricInstance(_identityConfig, _type);
        }

        public void LogDebug(string message, string name = "")
        {
            _telemetry.Log(new LogMessage($"{message}, from {name}", LogLevel.Debug, new LogOrigin(_fabricInstance), _tickRepository.GetLatestTick()));
        }

        public void LogException(Exception exception, string name = "")
        {
            _telemetry.Log(new LogMessage($"{exception}, from {name}", LogLevel.Error, new LogOrigin(_fabricInstance), _tickRepository.GetLatestTick()));
        }

        public void LogWarning(string warning, string name = "")
        {
            _telemetry.Log(new LogMessage($"{warning}, from {name}", LogLevel.Warning, new LogOrigin(_fabricInstance), _tickRepository.GetLatestTick()));
        }
    }
}