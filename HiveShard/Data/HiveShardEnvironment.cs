using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.Builder;

namespace Xcepto.HiveShard
{
    public class HiveShardEnvironment
    {
        private int _gridSize;
        private DeploymentType _deploymentType;
        private IServiceCollection _serviceCollection;

        internal HiveShardEnvironment(int gridSize, DeploymentType deploymentType, IServiceCollection serviceCollection)
        {
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