using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Workers.Initialization.Tests.Events;

namespace HiveShard.Workers.Initialization.Tests.Initializer;

public class TestShardInitializer: IInitializer
{
    private GlobalChunkConfig _config;

    public TestShardInitializer(GlobalChunkConfig config)
    {
        _config = config;
    }

    public Task Initialize(IInitializationTunnel tunnel)
    {
        for (int x = _config.MinChunk.XCoord; x <= _config.MaxChunk.XCoord; x++)
        {
            for (int y = _config.MinChunk.YCoord; y <= _config.MaxChunk.YCoord; y++)
            {
                foreach (var increment in Increments)
                {
                    tunnel.Send(new InitialDataEvent(increment), new Chunk(x,y));
                }
            }
        }

        return Task.CompletedTask;
    }
    
    public static List<int> Increments = new List<int>()
    {
        2, 5, 6, 12, 25
    };
}