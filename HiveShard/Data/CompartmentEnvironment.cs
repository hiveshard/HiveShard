using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Data;

public class CompartmentEnvironment
{
    public CompartmentEnvironment(CompartmentIdentifier identifier, IServiceCollection services, IEnumerable<Type> dependencies, Type entryPointType, IEnumerable<EmitterIdentity> containedEmitters)
    {
        Services = services;
        Dependencies = dependencies;
        EntryPointType = entryPointType;
        ContainedEmitters = containedEmitters;
        Identifier = identifier;
    }

    public IServiceCollection Services { get; }
    public IEnumerable<Type> Dependencies { get; }
    public CompartmentIdentifier Identifier { get; }
    public Type EntryPointType { get; }
    public IEnumerable<EmitterIdentity> ContainedEmitters { get; }
}