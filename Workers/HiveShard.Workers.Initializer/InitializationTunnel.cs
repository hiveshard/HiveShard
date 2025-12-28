using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabric;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;

namespace HiveShard.Workers.Initializer
{
    public class InitializationTunnel: IInitializationTunnel, IHiveShard
    {
        private ISimpleFabric _simpleFabric;
        private Dictionary<(Type, Chunk), long> _offsets = new();

        public InitializationTunnel(ISimpleFabric simpleFabric)
        {
            _simpleFabric = simpleFabric;
        }

        public void Send<TEvent>(TEvent e, Chunk chunk) where TEvent : IEvent
        {
            var index = (typeof(TEvent), chunk);

            _offsets[index]++;
            _simpleFabric.Send(index.Item1.FullName!, chunk, e);
        }

        public async Task FinalizeInitialization()
        {
            var newGuid = Guid.NewGuid();
            var hiveShardIdentity = new HiveShardIdentity(new Chunk(-1, -1), ShardType.From<InitializationTunnel>(), newGuid);
            foreach (var keyValuePair in _offsets)
            {
                var offset = new TopicPartitionOffset(keyValuePair.Key.Item1.FullName!, keyValuePair.Key.Item2, keyValuePair.Value);
                await _simpleFabric.Send("completed-ticks",
                    new CompletedTick(hiveShardIdentity, keyValuePair.Value, new[] { offset }, DateTime.Now));
            }
        }

        public void Process(float seconds)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}