namespace HiveShard.Data.Telemetry
{
    public class SpanWithPayload
    {
        public SpanWithPayload(string fullTypeName, string payload, HiveShardIdentity hiveShardIdentity, long tick)
        {
            FullTypeName = fullTypeName;
            Payload = payload;
            HiveShardIdentity = hiveShardIdentity;
            Tick = tick;
        }

        public string FullTypeName { get; }
        public string Payload { get; }
        public HiveShardIdentity HiveShardIdentity { get; }
        public long Tick { get; }
    }
}