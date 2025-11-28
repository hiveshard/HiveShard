using HiveShard.Interface;

namespace HiveShard.Workers.Edge;

public class EdgeWorker: IIsolatedEntryPoint
{
    public Task Start() => Task.CompletedTask;
}