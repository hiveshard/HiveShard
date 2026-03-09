using System;
using System.Collections.Generic;
using HiveShard.Interface.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Data;

public class ServiceEnvironment
{
    private readonly IEnumerable<(Type, CompartmentIdentifier)> _entryPointLocations;
    public GlobalChunkConfig GlobalChunkConfig { get; }
    public IServiceCollection Outer { get; }
    public IEnumerable<CompartmentEnvironment> Inner { get; }
    public IEventRepository EventRepository { get; }

    public ServiceEnvironment(GlobalChunkConfig globalChunkConfig,
        IServiceCollection serviceCollection,
        IEnumerable<CompartmentEnvironment> inner, 
        IEnumerable<(Type, CompartmentIdentifier)> entryPointLocations, IEventRepository eventRepository)
    {
        _entryPointLocations = entryPointLocations;
        EventRepository = eventRepository;
        Inner = inner;
        Outer = serviceCollection;
        GlobalChunkConfig = globalChunkConfig;
    }
}