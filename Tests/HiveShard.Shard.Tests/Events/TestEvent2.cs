using HiveShard.Interface;

namespace HiveShard.Shard.Tests.Events;

public class TestEvent2: IEvent
{
    public TestEvent2(int number)
    {
        Number = number;
    }

    public int Number { get; }
}