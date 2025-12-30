using System;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Repository;
using HiveShard.Repository;

namespace HiveShard.Builder;

public class EventBuilder
{
    private EventRepository _eventRepository;

    public EventBuilder(EventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public EventBuilder RegisterEvent<TEvent>(IEventEmitterType shardType)
        where TEvent : IEvent
    {
        _eventRepository.RegisterEvent<TEvent>(shardType);
        return this;
    }

    public EventBuilder InferAllEvents()
    {
        throw new NotImplementedException();
    }
}