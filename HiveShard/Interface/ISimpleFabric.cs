using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Data;

namespace HiveShard.Interface;

public interface ISimpleFabric: IFabric
{
    void Start(CancellationToken cancellationToken);
    void CompleteDeliveries(int deliveries);
    void Register<T>(string topic, EmitterIdentity consumer, Action<Consumption<IEnvelope<T>>> action) where T: IEvent;
    void Register<T>(string topic, Chunk chunk, EmitterIdentity consumer, Action<Consumption<IEnvelope<T>>> action) where T: IEvent;
    void Register<T>(string topic, Partition partition, EmitterIdentity consumer, Action<Consumption<IEnvelope<T>>> action) where T: IEvent;
    void Send<T>(string topic, IEnvelope<T> message) where T: IEvent;
    void Send<T>(string topic, Chunk chunk, IEnvelope<T> message) where T: IEvent;
    void Send<T>(string topic, Partition partition, IEnvelope<T> message) where T: IEvent;
    void Send<T>(string topic, Partition partition, Func<BatchedOffsetResults, IEnvelope<T>> messageBuilder) where T: IEvent;
    IEnumerable<Consumption<IEnvelope<object>>> FetchTopic(TopicChunk topicChunk, long fromOffset, long toOffsetExclusive);
    IEnumerable<Consumption<IEnvelope<object>>> FetchTopic(TopicPartition topicPartition, long fromOffset, long toOffsetExclusive);
}