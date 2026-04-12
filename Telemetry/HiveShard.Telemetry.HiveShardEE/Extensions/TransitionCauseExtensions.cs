using HiveShard.Data;
using HiveShardEE.API.Contracts.Data;

namespace HiveShard.Telemetry.HiveShardEE.Extensions;

public static class TransitionCauseExtensions
{
    public static Cause ToCause(this TransitionCause cause)
    {
        return new Cause(
            tick: cause.Tick,
            shardType: cause.ShardType,
            converge: cause.Converge,
            shardX: cause.ShardX,
            shardY: cause.ShardY,
            shardReplica: cause.ShardReplica,
            inboundEvent: cause.InboundEvent,
            inboundEventType: cause.InboundEventType,
            outboundEvent: cause.OutboundEvent,
            outboundEventType: cause.OutboundEventType
        );
    }
}