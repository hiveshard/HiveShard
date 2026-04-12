using System;
using HiveShard.Data;

namespace HiveShard.Interface;

public interface IEnvelope<out TEvent>
{
    public TEvent Payload { get; }
    public EmitterIdentity Emitter { get; }
    public Guid MessageId { get; }
}