namespace HiveShard.Data.Telemetry
{
    public class WorkerMetrics
    {
        public WorkerMetrics(long tick)
        {
            Tick = tick;
        }

        public long Tick { get; }
    }
}