using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Xcepto.Tests.Event;

namespace HiveShard.Xcepto.Tests.Initializer;

public class StateShardAllInitializer: IInitializer
{
    private readonly GlobalChunkConfig _config;

    public StateShardAllInitializer(GlobalChunkConfig config)
    {
        _config = config;
    }

    public void Initialize(IInitializationTunnel tunnel)
    {
        foreach (var chunk in _config.AllChunks)
        {
            tunnel.Send(new InitializationEvent(chunk), chunk);
        }
    }
}