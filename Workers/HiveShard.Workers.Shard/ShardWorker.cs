using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Providers;
using HiveShard.Interface.Repository;
using HiveShard.Shard;
using HiveShard.Shard.Interfaces;
using HiveShard.Workers.Shard.Data;
using HiveShard.Workers.Shard.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Workers.Shard;

public class ShardWorker: IIsolatedEntryPoint
{
    private readonly ShardAdditionRepository _shardAdditionRepository;
    private readonly HiveShardRepository _hiveShardRepository;
    private readonly IHiveShardTelemetry _loggingProvider;
    private readonly ITickRepository _tickRepository;
    private readonly ISerializer _serializer;
    private readonly ISimpleFabric _fabric;
    private readonly ICancellationProvider _cancellationProvider;
    private readonly GlobalChunkConfig _globalChunkConfig;
    private readonly IEventRepository _eventRepository;
        
    private readonly Dictionary<HiveShardIdentity, IScopedShardTunnel> _tunnels = new();

    public ShardWorker(
        ISimpleFabric fabric, 
        IHiveShardTelemetry loggingProvider, 
        ITickRepository tickRepository, 
        ISerializer serializer, 
        ShardAdditionRepository shardAdditionRepository, 
        ICancellationProvider cancellationProvider, HiveShardRepository hiveShardRepository, GlobalChunkConfig globalChunkConfig, IEventRepository eventRepository)
    {
        _fabric = fabric;
        _serializer = serializer;
        _shardAdditionRepository = shardAdditionRepository;
        _cancellationProvider = cancellationProvider;
        _hiveShardRepository = hiveShardRepository;
        _globalChunkConfig = globalChunkConfig;
        _eventRepository = eventRepository;
        _tickRepository = tickRepository;
        _loggingProvider = loggingProvider;
    }

    public Task Start()
    {
        while (!_cancellationProvider.GetToken().IsCancellationRequested)
        while (_shardAdditionRepository.TryRetrieve(out ShardAdditionRequest request))
        {
            var shardType = request.ShardIdentity.ShardType.GetShardType();
            var shardChunk = request.ShardIdentity.Chunk;
            var shardServiceProvider = new ServiceCollection()
                .AddSingleton(shardType)
                .AddSingleton<Chunk>(shardChunk)
                .AddSingleton<IEventRepository>(_eventRepository)
                .AddSingleton<GlobalChunkConfig>(_globalChunkConfig)
                .AddSingleton<IScopedShardTunnel, ScopedShardTunnel>()
                .AddSingleton<ICancellationProvider>(_cancellationProvider)
                .AddSingleton<ISimpleFabric>(_fabric)
                .AddSingleton<ITickRepository>(_tickRepository)
                .AddSingleton<ISerializer>(_serializer)
                .AddSingleton<HiveShardIdentity>(request.ShardIdentity)
                .AddSingleton<IHiveShardTelemetry>(_loggingProvider)
                .BuildServiceProvider();

            ScopedShardTunnel tunnel = (ScopedShardTunnel)shardServiceProvider.GetRequiredService<IScopedShardTunnel>();
            var hiveShard = (IHiveShard)shardServiceProvider.GetRequiredService(shardType);
            hiveShard.Initialize(shardChunk);
            tunnel.Initialize(hiveShard, request.ShardIdentity);
            _hiveShardRepository.AddHiveShard(request.ShardIdentity, shardServiceProvider);
            _tunnels.Add(request.ShardIdentity, tunnel);
        }

        return Task.CompletedTask;
    }
}