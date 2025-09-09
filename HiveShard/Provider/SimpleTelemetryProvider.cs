using System;
using HiveShard.Data.Telemetry;
using HiveShard.Interface;
using HiveShard.Interface.Logging;

namespace HiveShard.Provider
{
    public class SimpleTelemetryProvider: ITelemetryProvider
    {
        private IHiveShardSimpleLoggingProvider _loggingProvider;

        public SimpleTelemetryProvider(IHiveShardSimpleLoggingProvider loggingProvider)
        {
            _loggingProvider = loggingProvider;
        }

        public void Log(LogMessage logMessage)
        {
            string message;
            var origin = logMessage.LogOrigin;
            switch (origin.Type)
            {
                case LogOriginType.Shard:
                    message = $"{origin.HiveShardIdentity!.ShardType.ToString()}({origin.HiveShardIdentity.Chunk.Topic})[{logMessage.Tick}]: {logMessage.Message}";
                    break;
                case LogOriginType.Worker:
                    message = $"{origin.Worker!.Value}[{logMessage.Tick}]: {logMessage.Message}";
                    break;
                case LogOriginType.Fabric:
                    message = $"{origin.FabricInstance!.TypeString}[{logMessage.Tick}]: {logMessage.Message}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (logMessage.LogLevel)
            {
                case LogLevel.Debug:
                    _loggingProvider.LogDebug(message);
                    break;
                case LogLevel.Warning:
                    _loggingProvider.LogWarning(message);
                    break;
                case LogLevel.Error:
                    _loggingProvider.LogError(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Span(SpanWithPayload spanWithPayload)
        {
        }

        public void Metric(WorkerMetrics workerMetrics)
        {
            throw new System.NotImplementedException();
        }
    }
}