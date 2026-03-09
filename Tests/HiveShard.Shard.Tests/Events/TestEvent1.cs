using HiveShard.Interface;
using HiveShard.Shard.Tests.Data;

namespace HiveShard.Shard.Tests.Events;

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