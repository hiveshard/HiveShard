using HiveShard.Interface;

namespace HiveShard.Ticker.Tests.Events;

public class InitializationEvent : IEvent
{
    public int Data { get; }

    public InitializationEvent(int data)
    {
        Data = data;
    }
}