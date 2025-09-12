using DotChaser.Events;
using DotChaser.Maps;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabric.Ticker;
using HiveShard.Interface;

namespace DotChaser.Local;

public class DotChaserInitializer: IInitializer
{
    private ISimpleFabric _simpleFabric;

    public DotChaserInitializer(ISimpleFabric simpleFabric)
    {
        this._simpleFabric = simpleFabric;
    }

    public Task Initialize()
    {
        ElaborateMap elaborateMap = new ElaborateMap();

        var chunk = new Chunk(0, 0);
        var fullName = typeof(InitialGrid)!.FullName;
        _simpleFabric.Send(fullName, chunk,
            new InitialGrid(elaborateMap.Grid, elaborateMap.Height, elaborateMap.Width));
        _simpleFabric.Send("completed-ticks",
            new Tick(1, 0, [new TopicPartitionOffset(fullName, chunk, 1)], DateTime.Now));
        return Task.CompletedTask;
    }
}