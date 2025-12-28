using System;
using System.Threading.Tasks;

namespace HiveShard.Client.Interfaces
{
    public interface IClientTunnel
    {
        public Task RegisterHotPathEventHandler<TEvent>(Action<TEvent> handler);
        public void RegisterLocalEventHandler<TEvent>(Action<TEvent> handler);
        public void SendHotPathEvent(Type messageType, object message);
        public void SendLocalEvent(Type messageType, object message);
        public Task SendHotPathEvent<TType>(TType message);
        public void SendLocalEvent<TType>(TType message);
        public Task Connect(HiveShard.Data.HiveShardClient hiveShardClient);
    }
}