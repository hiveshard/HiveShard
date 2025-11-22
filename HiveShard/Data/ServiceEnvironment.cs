using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Data
{
    public class ServiceEnvironment
    {
        private int _gridSize;
        public IServiceCollection Outer { get; }
        public IEnumerable<WorkerEnvironment> Inner { get; }

        public ServiceEnvironment(
            int gridSize, 
            IServiceCollection serviceCollection,
            IEnumerable<WorkerEnvironment> inner)
        {
            Inner = inner;
            Outer = serviceCollection;
            _gridSize = gridSize;
        }

        public Task Start()
        {
            throw new System.NotImplementedException();
        }
    }
}