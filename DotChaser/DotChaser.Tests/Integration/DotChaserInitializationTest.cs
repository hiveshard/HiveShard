using DotChaser.Local;
using HiveShard.Ticker;
using HiveShard.Worker.Data;
using Xcepto;
using Xcepto.HiveShard.Adapters;
using Xcepto.HiveShard.Scenarios;

namespace DotChaser.Tests.Integration;

public class DotChaserInitializationTest
{
    [Test]
    public async Task Test()
    {
        await XceptoTest.Given(new InMemoryHiveShard(), builder =>
        {
            var ticker = builder.RegisterAdapter(new TickerXceptoAdapter(new TickerConfig(1)));
            var worker = builder.RegisterAdapter(new WorkerXceptoAdapter(new WorkerConfig(1)));
            worker.AddHiveShardStep<GridShard>();
            var initializer = builder.RegisterAdapter(new InitializerXceptoAdapter<DotChaserInitializer>());
        });
    }
}