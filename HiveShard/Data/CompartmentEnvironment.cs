using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Data;

public class CompartmentEnvironment
{
    public CompartmentEnvironment(string identifier, IServiceCollection services, IEnumerable<Type> dependencies)
    {
        Services = services;
        Dependencies = dependencies;
        Identifier = identifier;
    }

    public IServiceCollection Services { get; }
    public IEnumerable<Type> Dependencies { get; }
    public string Identifier { get; }
}