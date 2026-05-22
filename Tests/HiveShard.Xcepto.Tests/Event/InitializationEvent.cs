using HiveShard.Data;
using HiveShard.Interface;

namespace HiveShard.Xcepto.Tests.Event;

public class InitializationEvent: IEvent
{
    public InitializationEvent(Chunk target)
    {
        Target = target;
    }

    public Chunk Target { get; }
}