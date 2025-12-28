using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Fabric.Ticker;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Util;

namespace HiveShard.Fabrics.InMemory
{
    public class InMemorySimpleFabric: ISimpleFabric
    {
        private GlobalChunkConfig _globalChunkConfig;
        
        private ConcurrentDictionary<(Type, Partition), ConcurrentDictionary<long, Consumption<object>>> _topics = new();
        private ConcurrentDictionary<(Type, Partition), List<Action<Consumption<object>>>> _consumers = new();
        private ConcurrentDictionary<(Type, Partition), long> _topicMaxOffsets = new();
        private ConcurrentDictionary<Action<Consumption<object>>, long> _consumerOffsets = new();

        public InMemorySimpleFabric(IFabricLoggingProvider loggingProvider, IIdentityConfig identityConfig, GlobalChunkConfig globalChunkConfig)
        {
            _globalChunkConfig = globalChunkConfig;
            loggingProvider.GetScopedLogger<InMemoryEdgeFabric>(identityConfig);
        }

        public void Register<T>(string topic, Action<Consumption<T>> action)
            => Register(topic, new Chunk(0, 0), action);

        public void Register<T>(string topic, Chunk chunk, Action<Consumption<T>> action) =>
            Register(topic, chunk.ToPartition(_globalChunkConfig), action);

        public void Register<T>(string topic, Partition partition, Action<Consumption<T>> action)
        {
            var index = (typeof(T), partition);
            InitTopic(index);

            Action<Consumption<object>> newConsumer = o => action(new Consumption<T>((T)o.Message, o.Offset));
            _consumers[index].Add(newConsumer);
            _consumerOffsets[newConsumer] = 0;

            var messages = _topics[index];
            long currentOffset = _consumerOffsets[newConsumer];
            while (currentOffset < _topicMaxOffsets[index])
            {
                if (!messages.TryGetValue(currentOffset, out var value))
                    throw new Exception("offset not existent in topic");

                newConsumer(value);
                _consumerOffsets[newConsumer] = currentOffset;
                currentOffset += 1;
            }
        }

        public Task Send<T>(string topic, T message)
        {
            return Send<T>(topic, new Chunk(0, 0), message);
        }

        public Task Send<T>(string topic, Chunk chunk, T message) =>
            Send<T>(topic, chunk.ToPartition(_globalChunkConfig), message);

        public Task Send<T>(string topic, Partition partition, T message)
        {
            var index = (typeof(T), partition);

            InitTopic(index);
            
            
            var currentOffset = _topicMaxOffsets[index];
            var consumption = new Consumption<object>(message, currentOffset);
            _topics[index].TryAdd(currentOffset, consumption);

            var newOffset = currentOffset + 1;

            foreach (var action in _consumers[index])
            {
                action(consumption);
                _consumerOffsets[action] = newOffset;
            }

            _topicMaxOffsets[index] = newOffset;
            return Task.CompletedTask;
        }

        private void InitTopic((Type, Partition) index)
        {
            _topicMaxOffsets.GetOrAdd(index, _ => 0);
            _topics.GetOrAdd(index, _ => new ConcurrentDictionary<long, Consumption<object>>());
            _consumers.GetOrAdd(index, _ => new List<Action<Consumption<object>>>());
        }
    }
}
