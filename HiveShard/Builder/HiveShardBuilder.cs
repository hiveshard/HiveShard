using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Fabric.Client;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.Builder
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
        }

        public HiveShardBuilder(HiveShardEnvironment environment)
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

        public HiveShardEnvironment Build()
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
            
            return new HiveShardEnvironment(_defaultGridSize, _deploymentType, _serviceCollection);
        }
    }

    public enum DeploymentType
    {
        InMemory,
        ExternalFabric,
        HiveShardPlatform,
        StandaloneService
    }
}