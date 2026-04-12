using System;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Fabrics.InMemory.Data;

public class ConsumerRegistration
{
    public ConsumerRegistration(Action<Consumption<IEnvelope<object>>> action, EmitterIdentity consumerIdentity)
    {
        Action = action;
        ConsumerIdentity = consumerIdentity;
    }

    public Action<Consumption<IEnvelope<object>>> Action { get; }
    public EmitterIdentity ConsumerIdentity { get; }
}