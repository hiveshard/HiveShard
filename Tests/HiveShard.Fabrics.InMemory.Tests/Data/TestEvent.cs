using HiveShard.Interface;

namespace HiveShard.Fabrics.InMemory.Tests.Data;

public class TestEvent: IEvent
{
    public Guid Value { get; }

    public TestEvent(Guid value)
    {
        this.Value = value;
    }
}