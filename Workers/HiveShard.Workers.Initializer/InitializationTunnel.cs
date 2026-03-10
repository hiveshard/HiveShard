using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;namespace HiveShard.Workers.Initializer;

public class InitializationTunnel: IInitializationTunnel
{
    private readonly ISimpleFabric _simpleFabric;
    private readonly Dictionary<(Type, Chunk), long> _offsets = new();

    public InitializationTunnel(ISimpleFabric simpleFabric)
    {
        _simpleFabric = simpleFabric;
    }

    public void Send<TEvent>(TEvent e, Chunk chunk) where TEvent : IEvent
    {
        var index = (typeof(TEvent), chunk);

        _offsets[index]++;
        _simpleFabric.Send(index.Item1.FullName!, chunk, new Envelope<TEvent>(e, Guid.NewGuid()));
    }

    public async Task FinalizeInitialization()
    {
        var newGuid = Guid.NewGuid();
        var emitterIdentity = new InitializerType(new EmitterIdentity($"initializer-{newGuid}"));
        foreach (var offsetsPerTopic in _offsets
                     .GroupBy(x=>x.Key.Item1, 
                         x => (x.Key.Item2, x.Value)))
        {
            List<TopicPartitionOffset> offsets = new();
            foreach (var (chunk, offset) in offsetsPerTopic) offsets.Add(new TopicPartitionOffset(offsetsPerTopic.Key.FullName!, chunk, offset));
            _simpleFabric.Send("completed-ticks",
                new Envelope<CompletedTick>(
                    CompletedTick.From(offsetsPerTopic.Key, emitterIdentity, 0, offsets.AsEnumerable()),
                    Guid.NewGuid()
                )
            );
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