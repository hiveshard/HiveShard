using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Logging;

namespace HiveShard.Fabrics.InMemory;

public class InMemorySimpleFabric: ISimpleFabric
{
    private readonly GlobalChunkConfig _globalChunkConfig;
        
    private readonly ConcurrentDictionary<(EventType, Partition), ConcurrentDictionary<long, Consumption<IEnvelope<object>>>> _topics = new();
    private readonly ConcurrentDictionary<(EventType, Partition), List<Action<Consumption<IEnvelope<object>>>>> _consumers = new();
    private readonly ConcurrentDictionary<(EventType, Partition), long> _topicMaxOffsets = new();
    private readonly ConcurrentDictionary<Action<Consumption<IEnvelope<object>>>, long> _consumerOffsets = new();
    private readonly IHiveShardTelemetry _scopedFabricLoggingProvider;
    private readonly ISerializer _serializer;


    public InMemorySimpleFabric(IHiveShardTelemetry loggingProvider, GlobalChunkConfig globalChunkConfig, ISerializer serializer)
    {
        _globalChunkConfig = globalChunkConfig;
        _serializer = serializer;
        _scopedFabricLoggingProvider = loggingProvider;
    }

    public void Register<T>(string topic, Action<Consumption<IEnvelope<T>>> action) where T : IEvent =>
        Register(topic, new Partition(0), action);

    public void Register<T>(string topic, Chunk chunk, Action<Consumption<IEnvelope<T>>> action) where T : IEvent =>
        Register(topic, chunk.ToPartition(_globalChunkConfig), action);

    public void Register<T>(string topic, Partition partition, Action<Consumption<IEnvelope<T>>> action) where T : IEvent
    {
        var index = (EventType.From<T>(), partition);
        InitTopic(index);

        Action<Consumption<IEnvelope<object>>> newConsumer = o => action(
            new Consumption<IEnvelope<T>>(new Envelope<T>((T)o.Message.Payload, o.Message.MessageId), o.Offset)
        );
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

    public void Send<T>(string topic, IEnvelope<T> message) where T : IEvent =>
        Send<T>(topic, new Partition(0), message);

    public void Send<T>(string topic, Chunk chunk, IEnvelope<T> message) where T : IEvent => 
        Send<T>(topic, chunk.ToPartition(_globalChunkConfig), message);


    public void Send<T>(string topic, Partition partition, IEnvelope<T> message) where T : IEvent =>
        Send<T>(topic, partition, _ => message);

    public void Send<T>(string topic, Partition partition, Func<BatchedOffsetResults, IEnvelope<T>> messageBuilder) where T : IEvent
    {
        var index = (EventType.From<T>(), partition);

        InitTopic(index);

        var offsets = _topicMaxOffsets
            .ToDictionary(x =>
                    new TopicPartition(x.Key.Item1.EventTypeFullname, x.Key.Item2),
                x => x.Value);
        IEnvelope<T> actualMessage = messageBuilder(new BatchedOffsetResults(offsets));
            
        var currentOffset = _topicMaxOffsets[index];
        var consumption = new Consumption<IEnvelope<object>>(new Envelope<object>(actualMessage.Payload, actualMessage.MessageId), currentOffset);
        _topics[index].TryAdd(currentOffset, consumption);
            
        _scopedFabricLoggingProvider.LogDebug($"Send({topic}[partition: {partition.Value}, offset: {currentOffset}]) with {_serializer.Serialize(actualMessage)}");

        var newOffset = currentOffset + 1;
        var fetchedConsumers = _consumers[index].ToArray();
        foreach (var action in fetchedConsumers)
        {
            action(consumption);
            _consumerOffsets[action] = newOffset;
        }

        _topicMaxOffsets[index] = newOffset;
    }

    public IEnumerable<Consumption<IEnvelope<object>>> FetchTopic(TopicChunk topicChunk, long fromOffset,
        long toOffsetExclusive) =>
        FetchTopic(new TopicPartition(topicChunk.Topic, topicChunk.Chunk.ToPartition(_globalChunkConfig)), fromOffset,
            toOffsetExclusive);

    public IEnumerable<Consumption<IEnvelope<object>>> FetchTopic(TopicPartition topicPartition, long fromOffset, long toOffsetExclusive)
    {
        if(toOffsetExclusive - fromOffset <= 0)
            return [];
        var index = (new EventType(topicPartition.Topic), topicPartition.Partition);
        if (!_topics.TryGetValue(index, out var concurrentDictionary))
            return [];

        List<Consumption<IEnvelope<object>>> consumptions = new();
        for (long i = fromOffset; i < toOffsetExclusive; i++)
        {
            if (!concurrentDictionary.TryGetValue(i, out var value))
                break;
            consumptions.Add(value);
        }

        return consumptions;
    }

    private void InitTopic((EventType, Partition) index)
    {
        _topicMaxOffsets.GetOrAdd(index, _ => 0);
        _topics.GetOrAdd(index, _ => new ConcurrentDictionary<long, Consumption<IEnvelope<object>>>());
        _consumers.GetOrAdd(index, _ => new List<Action<Consumption<IEnvelope<object>>>>());
    }
}