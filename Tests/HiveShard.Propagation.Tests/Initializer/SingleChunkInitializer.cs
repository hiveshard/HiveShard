using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Propagation.Tests.Events;

namespace HiveShard.Propagation.Tests.Initializer;

public class SingleChunkInitializer: IInitializer
{
    public void Initialize(IInitializationTunnel tunnel)
    {
        tunnel.Send(new InitializingEvent(7), new Chunk(0,0));
    }
}