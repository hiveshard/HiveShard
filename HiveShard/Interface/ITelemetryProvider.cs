using HiveShard.Data.Telemetry;

namespace HiveShard.Interface
{
    public interface ITelemetryProvider
    {
        public void Log(LogMessage logMessage);
        public void Span(SpanWithPayload spanWithPayload);
        public void Metric(WorkerMetrics workerMetrics);
    }
}