using System;

namespace HiveShard.Data;

public class TransitionCause
{
    public long Tick { get; }
    public string ShardType { get; }
    public int Converge { get; }
    public int ShardX { get; }
    public int ShardY { get; }
    public int ShardReplica { get; }
    public Guid InboundEvent { get; }
    public string InboundEventType { get; }
    public Guid OutboundEvent { get; }
    public string OutboundEventType { get; }

    public TransitionCause(long tick,
        string shardType,
        int converge,
        int shardX,
        int shardY,
        int shardReplica,
        Guid inboundEvent,
        string inboundEventType,
        Guid outboundEvent,
        string outboundEventType)
    {
        Tick = tick;
        OutboundEventType = outboundEventType;
        OutboundEvent = outboundEvent;
        InboundEventType = inboundEventType;
        InboundEvent = inboundEvent;
        ShardReplica = shardReplica;
        ShardY = shardY;
        ShardX = shardX;
        Converge = converge;
        ShardType = shardType;
    }
}