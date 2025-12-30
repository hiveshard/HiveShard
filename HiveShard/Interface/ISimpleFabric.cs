using System;
using System.Threading.Tasks;
using HiveShard.Data;

namespace HiveShard.Interface
{
    public interface ISimpleFabric: IFabric
    {
        void Register<T>(string topic, Action<Consumption<T>> action) where T: IEvent;
        void Register<T>(string topic, Chunk chunk, Action<Consumption<T>> action) where T: IEvent;
        void Register<T>(string topic, Partition partition, Action<Consumption<T>> action) where T: IEvent;
        Task Send<T>(string topic, T message) where T: IEvent;
        Task Send<T>(string topic, Chunk chunk, T message) where T: IEvent;
        Task Send<T>(string topic, Partition partition, T message) where T: IEvent;
    }
}