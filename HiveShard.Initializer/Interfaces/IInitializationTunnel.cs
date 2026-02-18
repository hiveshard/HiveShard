using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Initializer.Interfaces;

public interface IInitializationTunnel
{
    public void Send<TEvent>(TEvent e, Chunk chunk) where TEvent: IEvent;
}