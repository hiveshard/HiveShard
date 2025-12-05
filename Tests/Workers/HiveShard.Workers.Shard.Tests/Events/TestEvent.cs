using HiveShard.Interface;

namespace HiveShard.ShardWorker.Tests.Events;

public class TestEvent: IEvent
{
    public TestEvent(int number)
    {
        Number = number;
    }

    public int Number { get; }
}