using System;
using System.Collections.Generic;
using HiveShard.Data;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Builder
{
    public class HiveShardBuilder
    {
        private readonly int _defaultGridSize = 1;
        private readonly DeploymentType _deploymentType;
        private List<Type> _shards = new();
        private IServiceCollection _serviceCollection;

        public HiveShardBuilder(int gridSize, DeploymentType deploymentType)
        {
            _deploymentType = deploymentType;
            _defaultGridSize = gridSize;
            _serviceCollection = new ServiceCollection();
        }

        public HiveShardBuilder(ServiceEnvironment environment)
        {
            _serviceCollection = environment.GetServices();
        }

        public HiveShardBuilder AddShard<T>()
        where T: class, IHiveShard
        {
            _shards.Add(typeof(T));
            return this;
        }

        public HiveShardBuilder AddWorkers<T>()
            where T: class, IHiveShard
        {
            _shards.Add(typeof(T));
            return this;
        }

        public ServiceEnvironment Build()
        {
            switch (_deploymentType)
            {
                case DeploymentType.InMemory:
                    // _serviceCollection.AddSingleton<ILoggingProvider, LoggingProvider>();
                    break;
                case DeploymentType.ExternalFabric:
                    break;
                case DeploymentType.HiveShardPlatform:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            foreach (var shard in _shards)
            {
                _serviceCollection.AddSingleton(shard);
            }
            
            return new ServiceEnvironment(_defaultGridSize, _deploymentType, _serviceCollection, []);
        }
    }
}