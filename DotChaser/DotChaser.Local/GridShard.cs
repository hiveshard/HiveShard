using System.Text;
using DotChaser.Events;
using HiveShard.Interface;
using HiveShard.Interface.Logging;

namespace DotChaser.Local;

public class GridShard: IHiveShard
{
    private IScopedShardTunnel _scopedShardTunnel;
    private IWorkerLoggingProvider _workerLoggingProvider;

    public GridShard(IScopedShardTunnel scopedShardTunnel, IWorkerLoggingProvider workerLoggingProvider)
    {
        _workerLoggingProvider = workerLoggingProvider;
        _scopedShardTunnel = scopedShardTunnel;

        _scopedShardTunnel.Register<InitialGrid>(x =>
        {
            StringBuilder gridString = new StringBuilder();
            for (int i = 0; i < x.Width; i++)
            {
                for (int j = 0; j < x.Height; j++)
                {
                    gridString.Append(x.Grid[i * x.Width + j]);
                }

                gridString.Append("\n");
            }
            
            _workerLoggingProvider.LogDebug($"Initial grid: {gridString}");
        });
    }

    public void Process(float seconds)
    {
        
    }

    public void Initialize()
    {
        
    }
}