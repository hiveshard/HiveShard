using HiveShard.Interface;

namespace HiveShard.Fabrics.Kafka.Tests.Data;

public class TestEvent: IEvent
{
    public TestEvent(int testNumber)
    {
        TestNumber = testNumber;
    }

    public int TestNumber { get; }
}