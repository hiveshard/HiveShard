using System;
using System.Collections.Generic;
using HiveShard.Interface.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Data;

public class ServiceEnvironment
{
    public GlobalChunkConfig GlobalChunkConfig { get; }
    public CompartmentEnvironment Outer { get; }
    public IEnumerable<CompartmentEnvironment> Inner { get; }
    public IEventRepository EventRepository { get; }

    public ServiceEnvironment(GlobalChunkConfig globalChunkConfig,
        CompartmentEnvironment outer,
        IEnumerable<CompartmentEnvironment> inner,
        IEventRepository eventRepository)
    {
        EventRepository = eventRepository;
        Inner = inner;
        Outer = outer;
        GlobalChunkConfig = globalChunkConfig;
    }
}