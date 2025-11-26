using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Data
{
    public class ServiceEnvironment
    {
        public int GridSize { get; }
        public IEnumerable<(string, Type, string)> EntryPointLocations { get; }
        public IServiceCollection Outer { get; }
        public IEnumerable<CompartmentEnvironment> Inner { get; }

        public ServiceEnvironment(int gridSize,
            IServiceCollection serviceCollection,
            IEnumerable<CompartmentEnvironment> inner, IEnumerable<(string, Type, string)> entryPointLocations)
        {
            EntryPointLocations = entryPointLocations;
            Inner = inner;
            Outer = serviceCollection;
            GridSize = gridSize;
        }

        public Task Start()
        {
            throw new System.NotImplementedException();
        }
    }
}