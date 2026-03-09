using HiveShard.Interface;

namespace HiveShard.Shard.Tests.Events;

public class TestEvent1: IEvent
{
    public TestEvent1(int number)
    {
        Number = number;
    }

    public int Number { get; }
}