using System;
using System.Collections.Generic;
using System.Linq;
using HiveShard.Interface;
using HiveShard.Ticker;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.Adapters;

namespace Xcepto.HiveShard.Builder
{
    public class HiveShardBuilder
    {
        private int _defaultGridSize = 1;
        private List<Type> _shards = new();
        private ServiceCollection _serviceCollection = new();
        private DeploymentType _deploymentType;

        public HiveShardBuilder(int gridSize, DeploymentType deploymentType)
        {
            _deploymentType = deploymentType;
            _defaultGridSize = gridSize;
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

        public HiveShardBuilder SetGridSize(int size)
        {
            _defaultGridSize = size;
            return this;
        }

        public HiveShardEnvironment Build()
        {
            return new HiveShardEnvironment();
        }
        
        public IServiceCollection GetServices()
        {
            return _serviceCollection;
        }
    }

    public enum DeploymentType
    {
        InMemory,
        ExternalFabric,
        HiveShardPlatform
    }
}