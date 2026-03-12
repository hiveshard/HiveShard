using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Data;

namespace HiveShard.Interface;

public interface ISimpleFabric: IFabric
{
    void Register<T>(string topic, Action<Consumption<IEnvelope<T>>> action) where T: IEvent;
    void Register<T>(string topic, Chunk chunk, Action<Consumption<IEnvelope<T>>> action) where T: IEvent;
    void Register<T>(string topic, Partition partition, Action<Consumption<IEnvelope<T>>> action) where T: IEvent;
    void Send<T>(string topic, IEnvelope<T> message) where T: IEvent;
    void Send<T>(string topic, Chunk chunk, IEnvelope<T> message) where T: IEvent;
    void Send<T>(string topic, Partition partition, IEnvelope<T> message) where T: IEvent;
    void Send<T>(string topic, Partition partition, Func<BatchedOffsetResults, IEnvelope<T>> messageBuilder) where T: IEvent;
    IEnumerable<Consumption<IEnvelope<object>>> FetchTopic(TopicChunk topicChunk, long fromOffset, long toOffsetExclusive);
    IEnumerable<Consumption<IEnvelope<object>>> FetchTopic(TopicPartition topicPartition, long fromOffset, long toOffsetExclusive);
}