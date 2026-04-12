
using HiveShard.Interface;

namespace HiveShard.Workers.Initialization.Tests.Events;

public class InitialDataEvent: IEvent
{
    public int Value { get; }

    public InitialDataEvent(int value)
    {
        Value = value;
    }
}