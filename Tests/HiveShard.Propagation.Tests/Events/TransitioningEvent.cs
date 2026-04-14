using HiveShard.Interface;

namespace HiveShard.Propagation.Tests.Events;

public class TransitioningEvent: IEvent
{
    public TransitioningEvent(int secret)
    {
        Secret = secret;
    }

    public int Secret { get; }
}