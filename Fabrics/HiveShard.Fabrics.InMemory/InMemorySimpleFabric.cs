using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Util;

namespace HiveShard.Fabrics.InMemory
{
    public class InMemorySimpleFabric: ISimpleFabric
    {
        private GlobalChunkConfig _globalChunkConfig;
        
        private ConcurrentDictionary<(EventType, Partition), ConcurrentDictionary<long, Consumption<object>>> _topics = new();
        private ConcurrentDictionary<(EventType, Partition), List<Action<Consumption<object>>>> _consumers = new();
        private ConcurrentDictionary<(EventType, Partition), long> _topicMaxOffsets = new();
        private ConcurrentDictionary<Action<Consumption<object>>, long> _consumerOffsets = new();

        public InMemorySimpleFabric(IFabricLoggingProvider loggingProvider, IIdentityConfig identityConfig, GlobalChunkConfig globalChunkConfig)
        {
            _globalChunkConfig = globalChunkConfig;
            loggingProvider.GetScopedLogger<InMemoryEdgeFabric>(identityConfig);
        }

        public void Register<T>(string topic, Action<Consumption<T>> action)
            where T: IEvent => Register(topic, new Chunk(0, 0), action);

        public void Register<T>(string topic, Chunk chunk, Action<Consumption<T>> action) 
            where T: IEvent => Register(topic, chunk.ToPartition(_globalChunkConfig), action);

        public void Register<T>(string topic, Partition partition, Action<Consumption<T>> action)
        where T: IEvent
        {
            var index = (EventType.From<T>(), partition);
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

        public Task Send<T>(string topic, T message) where T: IEvent
        {
            return Send<T>(topic, new Chunk(0, 0), message);
        }

        public Task Send<T>(string topic, Chunk chunk, T message) where T: IEvent =>
            Send<T>(topic, chunk.ToPartition(_globalChunkConfig), message);

        public Task Send<T>(string topic, Partition partition, T message) where T: IEvent
        {
            var index = (EventType.From<T>(), partition);

            InitTopic(index);
            
            
            var currentOffset = _topicMaxOffsets[index];
            var consumption = new Consumption<object>(message, currentOffset);
            _topics[index].TryAdd(currentOffset, consumption);

            var newOffset = currentOffset + 1;
            var fetchedConsumers = _consumers[index].ToArray();
            foreach (var action in fetchedConsumers)
            {
                action(consumption);
                _consumerOffsets[action] = newOffset;
            }

            _topicMaxOffsets[index] = newOffset;
            return Task.CompletedTask;
        }

        private void InitTopic((EventType, Partition) index)
        {
            _topicMaxOffsets.GetOrAdd(index, _ => 0);
            _topics.GetOrAdd(index, _ => new ConcurrentDictionary<long, Consumption<object>>());
            _consumers.GetOrAdd(index, _ => new List<Action<Consumption<object>>>());
        }
    }
}
