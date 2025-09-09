using HiveShard.Interface;

namespace HiveShard.Shard.Tests.Events;

public class TestEvent: IEvent
{
    public TestEvent(int number)
    {
        Number = number;
    }

    public int Number { get; }
}