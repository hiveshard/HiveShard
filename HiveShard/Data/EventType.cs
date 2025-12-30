using System;
using HiveShard.Interface;
using Newtonsoft.Json;

namespace HiveShard.Data;

public class EventType
{
    public string EventTypeFullname { get; }
    
    [JsonConstructor]
    public EventType(string eventTypeFullname)
    {
        EventTypeFullname = eventTypeFullname;
    }

    public static EventType From<T>()
        where T : IEvent => From(typeof(T));
    public static EventType From(Type eventType) =>
        new EventType(eventType.FullName!);

    protected bool Equals(EventType other)
    {
        return EventTypeFullname == other.EventTypeFullname;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EventType)obj);
    }

    public override int GetHashCode()
    {
        return EventTypeFullname.GetHashCode();
    }
}