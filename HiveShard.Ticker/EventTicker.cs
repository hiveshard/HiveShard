using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabric.Ticker;
using HiveShard.Interface;
using HiveShard.Interface.Logging;

namespace HiveShard.Ticker
{
    public class EventTicker
    {
        public EventTicker(ISimpleFabric simpleFabric, TickerConfig config, IWorkerLoggingProvider loggingProvider, IShardRepository shardRepository)
        {
            _loggingProvider = loggingProvider;
            _simpleFabric = simpleFabric;
            var topicPartitions = new List<TopicPartition>();
            var shards = new List<HiveShardIdentity>();
            throw new NotImplementedException();
            // for (int i = 0; i < config.N; i++)
            // {
            //     for (int j = 0; j < config.N; j++)
            //     {
            //         var chunk = new Chunk(i, j);
            //         foreach (ShardType shardType in shardRepository.GetShardTypes())
            //         {
            //             shards.Add(new HiveShardIdentity(chunk, shardType, Guid.NewGuid()));
            //         }
            //         topicPartitions.Add(new TopicPartition(config.EventType.FullName!, chunk));
            //     }
            // }

            _allTopics = topicPartitions.ToImmutableArray();
            _allShards = shards.ToImmutableArray();
            
            
        }

        private readonly ISimpleFabric _simpleFabric;
        private readonly IImmutableList<HiveShardIdentity> _allShards;
        private readonly IImmutableList<TopicPartition> _allTopics;
        private readonly Dictionary<HiveShardIdentity, CompletedTick> _completedTicks = new();
        private readonly IWorkerLoggingProvider _loggingProvider;

        private long _lastHandledTick;
        
        private void NextTick(long lastTick, DateTime lastTickTime)
        {
            var topicOffsets = _completedTicks
                .SelectMany(x => x.Value.ProduceOffsets
                    .Select(y => new KeyValuePair<TopicPartition, long>(new TopicPartition(y.Topic, y.Partition), y.Offset)));
            var maxOffsets = _allTopics.Select(topic =>
                {
                    var keyValuePairs = topicOffsets as KeyValuePair<TopicPartition, long>[] ?? topicOffsets.ToArray();
                    if (keyValuePairs.Length < 1 || !keyValuePairs.Any(x => x.Key.Equals(topic)))
                        return new KeyValuePair<TopicPartition, long>(topic, 0);
                    
                    return keyValuePairs
                        .Where(topicOffset => topicOffset.Key.Equals(topic))
                        .OrderByDescending(topicOffset => topicOffset.Value)
                        .First();
                })
                .ToImmutableDictionary();
            
            var elapsed = DateTime.Now - lastTickTime;
            var timeout = TimeSpan.FromSeconds(1) - elapsed;
            
            if (timeout > TimeSpan.Zero)
                Thread.Sleep(timeout);
            
            _lastHandledTick = lastTick + 1;
            _loggingProvider.LogDebug($"Send next tick: {_lastHandledTick}");
            var topicPartitionOffsets = maxOffsets.Select(
                x => new TopicPartitionOffset(x.Key.Topic, x.Key.Chunk, x.Value));
            _simpleFabric.Send("ticks", new Tick(_lastHandledTick, 1, topicPartitionOffsets, DateTime.Now));
            _completedTicks.Clear();
        }

        private void HandleCompletedTicks(Consumption<CompletedTick> obj)
        {
            if(obj.Message.Number< _lastHandledTick)
                return;
            _loggingProvider.LogDebug($"Completed tick arrived: {obj.Message.Number}");
            _completedTicks[obj.Message.HiveShardIdentity] = obj.Message;
            if (_allShards.All(x => _completedTicks.ContainsKey(x)))
                NextTick(obj.Message.Number, obj.Message.LastTickTime);
        }

        private static IEnumerable<Type> GetAllTypesImplementingInterface<TInterface>()
        {
            var interfaceType = typeof(TInterface);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x =>
            {
                var list = new List<Assembly>();
                list.Add(x);
                foreach (AssemblyName referencedAssembly in x.GetReferencedAssemblies())
                {
                    list.Add(Assembly.Load(referencedAssembly));
                }

                return list;
            });

            ISet<Type> uniqueTypes = new HashSet<Type>();
            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray(); // Skip broken ones
                }

                foreach (var type in types)
                {
                    if (type == null || !type.IsClass || type.IsAbstract)
                        continue;

                    if (interfaceType.IsAssignableFrom(type))
                        uniqueTypes.Add(type);
                }
            }

            return uniqueTypes;
        }

        public void Start()
        {
            _simpleFabric.Register<CompletedTick>("completed-ticks", HandleCompletedTicks);
        }
    }
}