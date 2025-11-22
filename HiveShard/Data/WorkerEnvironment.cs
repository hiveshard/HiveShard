using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Data;

public class WorkerEnvironment
{
    public WorkerEnvironment(IServiceCollection services, List<Type> dependencies)
    {
        Services = services;
        Dependencies = dependencies;
    }

    public IServiceCollection Services { get; }
    public List<Type> Dependencies { get; }
}