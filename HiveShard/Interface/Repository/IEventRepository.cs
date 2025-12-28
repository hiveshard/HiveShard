using System;
using System.Collections.Generic;
using HiveShard.Data;

namespace HiveShard.Interface.Repository;

public interface IEventRepository
{
    public int GetEventOrder<T>() where T : IEvent;
    int GetEventOrder(Type configEventType);
    public KeyValuePair<Type, int>[] GetTotalOrder();
}