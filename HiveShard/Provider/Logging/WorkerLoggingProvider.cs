using System;
using HiveShard.Data.Telemetry;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;

namespace HiveShard.Provider.Logging
{
    public class WorkerLoggingProvider: IWorkerLoggingProvider
    {
        private ITelemetryProvider _telemetryProvider;
        private ITickRepository _tickRepository;
        private IIdentityConfig _identityConfig;

        public WorkerLoggingProvider(ITelemetryProvider telemetryProvider, ITickRepository tickRepository, IIdentityConfig identityConfig)
        {
            _identityConfig = identityConfig;
            _telemetryProvider = telemetryProvider;
            _tickRepository = tickRepository;
        }

        public void LogDebug(string message, LogOrigin logOrigin)
        {
            _telemetryProvider.Log(new LogMessage(message, LogLevel.Debug, logOrigin, _tickRepository.GetLatestTick()));
        }

        public void LogDebug(string message)
        {
            _telemetryProvider.Log(new LogMessage(message, LogLevel.Debug, new LogOrigin(_identityConfig.GetIdentity()), _tickRepository.GetLatestTick()));
        }

        public void LogWarning(string message, LogOrigin logOrigin)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string message)
        {
            throw new NotImplementedException();
        }

        public void LogError(string message, LogOrigin logOrigin)
        {
            throw new NotImplementedException();
        }

        public void LogError(Exception exception, LogOrigin logOrigin)
        {
            _telemetryProvider.Log(new LogMessage(exception.ToString() ,LogLevel.Error, logOrigin, _tickRepository.GetLatestTick()));

        }
    }
}