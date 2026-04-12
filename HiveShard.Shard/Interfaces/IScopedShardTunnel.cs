using System;
using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Shard.Interfaces;

public interface IScopedShardTunnel
{
    public void Register<TEvent>(Action<Message<TEvent>> handler) where TEvent: IEvent;
    public void Send<TEvent>(TEvent message) where TEvent: IEvent;
}