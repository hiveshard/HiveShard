using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Tests.Utils;

public static class FabricExtensions
{
    public static void Send<T>(this ISimpleFabric fabric, string topic, Chunk chunk, T message)
    where T: IEvent =>
        fabric.Send(topic, chunk, new Envelope<T>(message, Guid.NewGuid()));
    
    public static void Send<T>(this ISimpleFabric fabric, string topic, Partition partition, T message)
        where T: IEvent =>
        fabric.Send(topic, partition, new Envelope<T>(message, Guid.NewGuid()));
}