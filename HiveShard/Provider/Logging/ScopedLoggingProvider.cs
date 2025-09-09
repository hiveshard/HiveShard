using HiveShard.Data;
using HiveShard.Data.Telemetry;
using HiveShard.Interface.Logging;

namespace HiveShard.Provider.Logging
{
    public class ScopedLoggingProvider: IShardLoggingProvider
    {
        private IWorkerLoggingProvider _loggingProvider;
        private HiveShardIdentity _hiveShardIdentity;

        public ScopedLoggingProvider(IWorkerLoggingProvider loggingProvider, HiveShardIdentity hiveShardIdentity)
        {
            _loggingProvider = loggingProvider;
            _hiveShardIdentity = hiveShardIdentity;
        }

        public void LogDebug(string message)
        {
            _loggingProvider.LogDebug(message, new LogOrigin(_hiveShardIdentity));
        }

        public void Warning(string message)
        {
            _loggingProvider.LogWarning(message, new LogOrigin(_hiveShardIdentity));
        }
    }
}