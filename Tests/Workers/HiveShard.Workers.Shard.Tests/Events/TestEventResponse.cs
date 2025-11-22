using HiveShard.Interface;

namespace HiveShard.ShardWorker.Tests.Events;

public class TestEventResponse: IEvent
{
    public TestEventResponse(int number)
    {
        Number = number;
    }

    public int Number { get; }
}