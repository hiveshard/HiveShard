using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Fabric.Ticker;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Shard;
using HiveShard.Workers.Shard.Data;
using HiveShard.Workers.Shard.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Workers.Shard
{
    public class ShardWorker: IIsolatedEntryPoint
    {
        private ShardAdditionRepository _shardAdditionRepository;
        private HiveShardRepository _hiveShardRepository;
        private IWorkerLoggingProvider _loggingProvider;
        private ITickRepository _tickRepository;
        private ISerializer _serializer;
        private ISimpleFabric _fabric;
        private ICancellationProvider _cancellationProvider;

        public ShardWorker(
            ISimpleFabric fabric, 
            IWorkerLoggingProvider loggingProvider, 
            ITickRepository tickRepository, 
            ISerializer serializer, 
            ShardAdditionRepository shardAdditionRepository, 
            ICancellationProvider cancellationProvider, HiveShardRepository hiveShardRepository)
        {
            _fabric = fabric;
            _serializer = serializer;
            _shardAdditionRepository = shardAdditionRepository;
            _cancellationProvider = cancellationProvider;
            _hiveShardRepository = hiveShardRepository;
            _tickRepository = tickRepository;
            _loggingProvider = loggingProvider;
        }

        public Task Start()
        {
            while (!_cancellationProvider.GetToken().IsCancellationRequested)
            {
                while (_shardAdditionRepository.TryRetrieve(out ShardAdditionRequest request))
                {
                    var shardType = request.ShardIdentity.ShardType.GetShardType();
                    var shardChunk = request.ShardIdentity.Chunk;
                    var shardServiceProvider = new ServiceCollection()
                        .AddSingleton(shardType)
                        .AddSingleton<Chunk>(shardChunk)
                        .AddSingleton<IScopedShardTunnel, ScopedShardTunnel>()
                        .AddSingleton<ISimpleFabric>(_fabric)
                        .AddSingleton<ITickRepository>(_tickRepository)
                        .AddSingleton<ISerializer>(_serializer)
                        .AddSingleton<HiveShardIdentity>(request.ShardIdentity)
                        .AddSingleton<IWorkerLoggingProvider>(_loggingProvider)
                        .BuildServiceProvider();

                    IScopedShardTunnel tunnel = shardServiceProvider.GetRequiredService<IScopedShardTunnel>();
                    var hiveShard = (IHiveShard)shardServiceProvider.GetRequiredService(shardType);
                    hiveShard.Initialize();
                    tunnel.Initialize(hiveShard);
                    _hiveShardRepository.AddHiveShard(request.ShardIdentity, shardServiceProvider);
                    var hashCode = RuntimeHelpers.GetHashCode(_hiveShardRepository);
                }
            }
            return Task.CompletedTask;
        }
    }
}