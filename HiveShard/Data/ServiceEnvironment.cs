using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Interface.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Data
{
    public class ServiceEnvironment
    {
        public GlobalChunkConfig GlobalChunkConfig { get; }
        public IEnumerable<(string, Type, string)> EntryPointLocations { get; }
        public IServiceCollection Outer { get; }
        public IEnumerable<CompartmentEnvironment> Inner { get; }
        public IEventRepository EventRepository { get; }

        public ServiceEnvironment(GlobalChunkConfig globalChunkConfig,
            IServiceCollection serviceCollection,
            IEnumerable<CompartmentEnvironment> inner, IEnumerable<(string, Type, string)> entryPointLocations, IEventRepository eventRepository)
        {
            EntryPointLocations = entryPointLocations;
            EventRepository = eventRepository;
            Inner = inner;
            Outer = serviceCollection;
            GlobalChunkConfig = globalChunkConfig;
        }

        public Task Start()
        {
            throw new System.NotImplementedException();
        }
    }
}