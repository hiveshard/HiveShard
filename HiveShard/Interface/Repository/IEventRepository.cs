using System;
using System.Collections.Generic;
using HiveShard.Data;

namespace HiveShard.Interface.Repository;

public interface IEventRepository
{
    public int GetEventOrder<T>() where T : IEvent;
    public int GetEventOrder(Type configEventType);
    public int GetEventOrder(string eventType);
    public IEnumerable<string> GetInitializationOnlyEvents();
    public KeyValuePair<string, int>[] GetTotalOrder();
    public IEventEmitterType[] GetEmitters(string eventType);
    public string[] GetTopicsOfEmitter(EmitterIdentity identity);
}