using HiveShard.Interface;
using HiveShard.Worker.Tests.Data;

namespace HiveShard.Worker.Tests.Events;

public class TestEvent2: IEvent
{
    public TestEvent2(int number, Operation operation = Operation.Addition)
    {
        Number = number;
        Operation = operation;
    }

    public int Number { get; }
    public Operation Operation { get; }
}