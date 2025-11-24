namespace HiveShard.Fabrics.Kafka.Tests.Data;

public class TestEvent
{
    public TestEvent(int testNumber)
    {
        TestNumber = testNumber;
    }

    public int TestNumber { get; }
}