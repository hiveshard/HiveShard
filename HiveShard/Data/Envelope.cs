using System;
using HiveShard.Interface;

namespace HiveShard.Data;

public sealed class Envelope<TEvent>: IEnvelope<TEvent>
{
    public Envelope(TEvent payload, Guid messageId)
    {
        Payload = payload;
        MessageId = messageId;
    }

    public TEvent Payload { get; }
    public Guid MessageId { get; }
}