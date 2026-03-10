using System;

namespace HiveShard.Workers.Initializer.Data;

public class InitializerAdditionRequest
{
    public InitializerEmitterIdentity EmitterIdentity { get; }
    public Type Type { get; }

    public InitializerAdditionRequest(InitializerEmitterIdentity initializerEmitterIdentity, Type type)
    {
        EmitterIdentity = initializerEmitterIdentity;
        Type = type;
    }
}