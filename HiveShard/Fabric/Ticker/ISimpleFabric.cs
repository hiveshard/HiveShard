using System;
using System.Threading.Tasks;
using HiveShard.Data;

namespace HiveShard.Fabric.Ticker
{
    public interface ISimpleFabric: IFabric
    {
        void Register<T>(string topic, Action<Consumption<T>> action);
        void Register<T>(string topic, Chunk chunk, Action<Consumption<T>> action);
        Task Send<T>(string topic, T message);
        Task Send<T>(string topic, Chunk chunk, T message);
    }
}