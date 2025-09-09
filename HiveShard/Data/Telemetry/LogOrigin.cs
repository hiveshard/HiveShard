using System;

namespace HiveShard.Data.Telemetry
{
    public class LogOrigin
    {
        public LogOrigin(HiveShardIdentity hiveShardIdentity)
        {
            HiveShardIdentity = hiveShardIdentity;
            Type = LogOriginType.Shard;
        }
        public LogOrigin(Guid worker)
        {
            Worker = worker;
            Type = LogOriginType.Worker;
        }

        public LogOrigin(FabricInstance fabric)
        {
            FabricInstance = fabric;
            Type = LogOriginType.Fabric;
        }

        public HiveShardIdentity? HiveShardIdentity { get; } = null;
        public Guid? Worker { get; } = Guid.Empty;
        public FabricInstance? FabricInstance { get; } = null;
        public LogOriginType Type { get; }
    }
}