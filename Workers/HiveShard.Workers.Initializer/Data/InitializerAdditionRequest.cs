using System;
using System.Collections.Generic;

namespace HiveShard.Workers.Initializer.Data;

public class InitializerAdditionRequest
{
    public InitializerEmitterIdentity EmitterIdentity { get; }
    public Type Type { get; }
    public IEnumerable<InitializerDependency> InitializerDependencies { get; }

    public InitializerAdditionRequest(InitializerEmitterIdentity initializerEmitterIdentity, Type type, IEnumerable<InitializerDependency> initializerDependencies)
    {
        EmitterIdentity = initializerEmitterIdentity;
        Type = type;
        InitializerDependencies = initializerDependencies;
    }
}