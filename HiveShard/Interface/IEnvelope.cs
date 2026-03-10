using System;

namespace HiveShard.Interface;

public interface IEnvelope<out TEvent>
{
    public TEvent Payload { get; }
    public Guid MessageId { get; }
}