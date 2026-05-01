using HiveShard.Interface;

namespace HiveShard.Workers.Initialization.Tests.Events;

public class DependencySecretEvent: IEvent
{
    public DependencySecretEvent(int secret)
    {
        Secret = secret;
    }

    public int Secret { get; }
}