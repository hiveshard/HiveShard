using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;

namespace HiveShard.Workers.Initializer
{
    public class InitializationTunnel: IInitializationTunnel
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
            var emitterIdentity = new InitializerType($"initializer-{newGuid}");
            foreach (var offsetsPerTopic in _offsets
                         .GroupBy(x=>x.Key.Item1, 
                             x => (x.Key.Item2, x.Value)))
            {
                List<TopicPartitionOffset> offsets = new();
                foreach (var (chunk, offset) in offsetsPerTopic)
                {
                    offsets.Add(new TopicPartitionOffset(offsetsPerTopic.Key.FullName!, chunk, offset));
                }
                await _simpleFabric.Send("completed-ticks",
                    CompletedTick.From(offsetsPerTopic.Key, emitterIdentity, 0, offsets.AsEnumerable()));
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