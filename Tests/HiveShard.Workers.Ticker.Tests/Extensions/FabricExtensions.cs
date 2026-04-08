using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Workers.Ticker.Tests.Extensions;

public static class FabricExtensions
{
    public static void Send<T>(this ISimpleFabric fabric, string topic, Chunk chunk, T message)
    where T: IEvent =>
        fabric.Send(topic, chunk, new Envelope<T>(message, Guid.NewGuid()));
    
    public static void Send<T>(this ISimpleFabric fabric, string topic, Partition partition, T message)
        where T: IEvent =>
        fabric.Send(topic, partition, new Envelope<T>(message, Guid.NewGuid()));
    
    public static void Send<T>(this ISimpleFabric fabric, string topic, T message)
        where T: IEvent =>
        fabric.Send(topic, new Partition(0), new Envelope<T>(message, Guid.NewGuid()));

    public static T FetchTopicOffset<T>(this ISimpleFabric fabric, Partition partition, long offset)
    where T: IEvent
    {
        var message = fabric.FetchTopic(new TopicPartition(typeof(T).FullName!, partition), offset, offset+1)
            .FirstOrDefault();
        if (message is null)
            throw new Exception($"There was no message at offset {offset} on {typeof(T).FullName!}[partition: {partition.Value}, offset: {offset}]");

        return (T)message.Message.Payload;
    }
}