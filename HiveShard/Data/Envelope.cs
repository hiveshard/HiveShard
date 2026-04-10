using System;
using HiveShard.Interface;

namespace HiveShard.Data;

public sealed class Envelope<TEvent>: IEnvelope<TEvent>
{
    public Envelope(TEvent payload, Guid messageId, EmitterIdentity emitter)
    {
        Payload = payload;
        MessageId = messageId;
        Emitter = emitter;
    }

    public TEvent Payload { get; }
    public EmitterIdentity Emitter { get; }
    public Guid MessageId { get; }
}