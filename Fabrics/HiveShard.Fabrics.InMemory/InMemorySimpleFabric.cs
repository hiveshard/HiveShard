using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Fabric.Ticker;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;

namespace HiveShard.Fabrics.InMemory
{
    public class InMemorySimpleFabric: ISimpleFabric
    {
        
        private Dictionary<(Type, Chunk), List<Action<Consumption<object>>>> _actions = new Dictionary<(Type, Chunk), List<Action<Consumption<object>>>>();
        private Dictionary<(Type, Chunk), long> _offsets = new Dictionary<(Type, Chunk), long>();
        private IScopedFabricLoggingProvider _scopedLogger;

        public InMemorySimpleFabric(IFabricLoggingProvider loggingProvider, IIdentityConfig identityConfig)
        {
            _scopedLogger = loggingProvider.GetScopedLogger<InMemoryEdgeFabric>(identityConfig);
        }

        public void Register<T>(string topic, Action<Consumption<T>> action)
            => Register(topic, new Chunk(0, 0), action);

        public void Register<T>(string topic, Chunk chunk, Action<Consumption<T>> action)
        {
            var index = (typeof(T), chunk);
            if (!_actions.ContainsKey(index) || !_offsets.ContainsKey(index))
            {
                _actions[index] = new List<Action<Consumption<object>>>();
                _offsets[index] = 0;
            }
            
            _actions[index].Add(o => action(new Consumption<T>((T)o.Message, o.Offset)));
        }

        public Task Send<T>(string topic, T message)
        {
            return Send<T>(topic, new Chunk(0, 0), message);
        }

        public Task Send<T>(string topic, Chunk chunk, T message)
        {
            var index = (typeof(T), chunk);
            if (!_actions.ContainsKey(index) || !_offsets.ContainsKey(index))
            {
                var exception = new Exception($"{index} was not consumed yet");
                _scopedLogger.LogException(exception);
                throw exception;
            }

            var newOffset = _offsets[index] + 1;

            foreach (var action in _actions[index])
            {
                action(new Consumption<object>(message, newOffset));
            }

            _offsets[index] = newOffset;
            return Task.CompletedTask;
        }
    }
}