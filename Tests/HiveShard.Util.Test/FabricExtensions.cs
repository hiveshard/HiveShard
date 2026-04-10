using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Util.Test;

public static class FabricExtensions
{
    public static void Send<T>(this ISimpleFabric fabric, string topic, Chunk chunk, EmitterIdentity emitterIdentity, T message)
    where T: IEvent =>
        fabric.Send(topic, chunk, new Envelope<T>(message, Guid.NewGuid(), emitterIdentity));
    
    public static void Send<T>(this ISimpleFabric fabric, string topic, Partition partition, EmitterIdentity emitterIdentity, T message)
        where T: IEvent =>
        fabric.Send(topic, partition, new Envelope<T>(message, Guid.NewGuid(), emitterIdentity));
}