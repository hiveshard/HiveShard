using System;
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
        
        private Dictionary<(Type, Chunk), Dictionary<long, Consumption<object>>> _topics = new();
        private Dictionary<(Type, Chunk), List<Action<Consumption<object>>>> _consumers = new();
        private Dictionary<(Type, Chunk), long> _topicMaxOffsets = new();
        private Dictionary<Action<Consumption<object>>, long> _consumerOffsets = new();

        public InMemorySimpleFabric(IFabricLoggingProvider loggingProvider, IIdentityConfig identityConfig)
        {
            loggingProvider.GetScopedLogger<InMemoryEdgeFabric>(identityConfig);
        }

        public void Register<T>(string topic, Action<Consumption<T>> action)
            => Register(topic, new Chunk(0, 0), action);

        public void Register<T>(string topic, Chunk chunk, Action<Consumption<T>> action)
        {
            var index = (typeof(T), chunk);
            if (!_topicMaxOffsets.ContainsKey(index))
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

        public Task Send<T>(string topic, Chunk chunk, T message)
        {
            var index = (typeof(T), chunk);

            if (!_topicMaxOffsets.ContainsKey(index))
                InitTopic(index);
            
            
            var currentOffset = _topicMaxOffsets[index];
            var consumption = new Consumption<object>(message, currentOffset);
            _topics[index].Add(currentOffset, consumption);

            var newOffset = currentOffset + 1;

            foreach (var action in _consumers[index])
            {
                action(consumption);
                _consumerOffsets[action] = newOffset;
            }

            _topicMaxOffsets[index] = newOffset;
            return Task.CompletedTask;
        }

        private void InitTopic((Type, Chunk) index)
        {
            _topicMaxOffsets[index] = 0;
            _topics[index] = new Dictionary<long, Consumption<object>>();
            _consumers[index] = new List<Action<Consumption<object>>>();
        }
    }
}