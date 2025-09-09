using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Fabric.Ticker;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Shard;
using HiveShard.Worker.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Worker
{
    public class AllInOneWorker
    {
        private readonly ConcurrentBag<Task> _ticks = new ConcurrentBag<Task>();
        
        private int _nChunks;
        private IWorkerLoggingProvider _loggingProvider;
        private ICancellationProvider _cancellationProvider;
        private ITickRepository _tickRepository;
        private ISerializer _serializer;
        private ISimpleFabric _fabric;

        public AllInOneWorker(ISimpleFabric fabric, IWorkerLoggingProvider loggingProvider, WorkerConfig workerConfig, ICancellationProvider cancellationProvider, ITickRepository tickRepository, ISerializer serializer)
        {
            _fabric = fabric;
            _serializer = serializer;
            _tickRepository = tickRepository;
            _cancellationProvider = cancellationProvider;
            _loggingProvider = loggingProvider;
            _nChunks = workerConfig.N;
        }

        public async Task Start()
        {
            await Task.WhenAll(_ticks);
        }

        public void AddHiveShard<T>()
        where T: class, IHiveShard
        {
            for (int i = 0; i < _nChunks; i++)
            {
                for (int j = 0; j < _nChunks; j++)
                {
                    var chunk = new Chunk(i,j);
                    var shardServiceProvider = new ServiceCollection()
                        .AddSingleton<T>()
                        .AddSingleton<Chunk>(chunk)
                        .AddSingleton<IScopedShardTunnel, ScopedShardTunnel>()
                        .AddSingleton<ISimpleFabric>(_fabric)
                        .AddSingleton<ITickRepository>(_tickRepository)
                        .AddSingleton<ISerializer>(_serializer)
                        .AddSingleton<HiveShardIdentity>(new HiveShardIdentity(chunk, new ShardType(typeof(T).FullName)))
                        .AddSingleton<IWorkerLoggingProvider>(_loggingProvider)
                        .BuildServiceProvider();

                    IScopedShardTunnel tunnel = shardServiceProvider.GetRequiredService<IScopedShardTunnel>();
                    var hiveShard = shardServiceProvider.GetRequiredService<T>();
                    hiveShard.Initialize();
                    tunnel.Initialize(hiveShard);
                    
                    _ticks.Add(tunnel.Start(_cancellationProvider.GetToken()));
                }
            }
        }
    }
}