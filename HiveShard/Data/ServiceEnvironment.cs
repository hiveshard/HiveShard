using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Data
{
    public class ServiceEnvironment
    {
        private int _gridSize;
        private DeploymentType _deploymentType;
        private IServiceCollection _serviceCollection;
        private IEnumerable<WorkerEnvironment> _workerEnvironments;

        internal ServiceEnvironment(
            int gridSize, 
            DeploymentType deploymentType, 
            IServiceCollection serviceCollection,
            IEnumerable<WorkerEnvironment> workerEnvironments)
        {
            _workerEnvironments = workerEnvironments;
            _serviceCollection = serviceCollection;
            _deploymentType = deploymentType;
            _gridSize = gridSize;
        }

        public IServiceCollection GetServices() => _serviceCollection;

        public Task Start()
        {
            throw new System.NotImplementedException();
        }
    }
}