using System.Threading.Tasks;
using HiveShard.Data.Telemetry;
using HiveShard.Fabric.Telemetry;
using HiveShard.Interface;

namespace HiveShard.Provider
{
    public class TelemetryProvider: ITelemetryProvider
    {
        private ITelemetryFabric _simpleFabric;

        public TelemetryProvider(ITelemetryFabric simpleFabric)
        {
            _simpleFabric = simpleFabric;
        }

        public void Log(LogMessage logMessage)
        {
            Task.Run(async () =>
            {
                if (logMessage.LogOrigin is { Type: LogOriginType.Shard, HiveShardIdentity: not null })
                    await _simpleFabric.Send("log", logMessage.LogOrigin.HiveShardIdentity.Chunk, logMessage);
                else
                    await _simpleFabric.Send("log-worker", logMessage);
            });
        }

        public void Span(SpanWithPayload spanWithPayload)
        {
            Task.Run(async () =>
            {
                await _simpleFabric.Send("span", spanWithPayload.HiveShardIdentity.Chunk, spanWithPayload);
            });
        }

        public void Metric(WorkerMetrics workerMetrics)
        {
            Task.Run(async () =>
            {
                await _simpleFabric.Send("metric", workerMetrics);
            });
        }
    }
}