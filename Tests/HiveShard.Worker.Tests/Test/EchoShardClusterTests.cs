using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Worker.Data;
using HiveShard.Worker.Tests.Events;
using HiveShard.Worker.Tests.Scenarios;
using HiveShard.Worker.Tests.Shards;
using Xcepto;
using Xcepto.HiveShard.Adapters;
using Xcepto.HiveShard.Extensions;

namespace HiveShard.Worker.Tests.Test;

[TestFixture(typeof(InMemoryScenario))]
[TestFixture(typeof(KafkaScenario))]
[TestFixture(typeof(HiveShardPlatformScenario))]
public class EchoShardClusterTests<T>
where T: XceptoScenario, new()
{
    [Test]
    [Ignore("No consistency in failures")]
    public async Task EchoShardResponseWithNumber()
    {
        await XceptoTest.Given(new T(), builder =>
        {
            var hiveShard = builder.RegisterHiveShard(x => x
                .AddShard<EchoHiveShard>()
                .SetGridSize(2)
                .Build());

            hiveShard.SendEdgeMessage();
            hiveShard.SendShardMessage();
        });
    }
}