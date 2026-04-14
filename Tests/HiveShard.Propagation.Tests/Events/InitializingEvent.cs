using HiveShard.Interface;

namespace HiveShard.Propagation.Tests.Events;

public class InitializingEvent: IEvent
{
    public InitializingEvent(int secret)
    {
        Secret = secret;
    }

    public int Secret { get; }
}