using HiveShard.Interface;
using HiveShard.Worker.Tests.Data;

namespace HiveShard.Worker.Tests.Events;

public class TestEvent1: IEvent
{
    public TestEvent1(int number, Operation operation = Operation.Addition)
    {
        Number = number;
        Operation = operation;
    }

    public int Number { get; }
    public Operation Operation { get; }
}