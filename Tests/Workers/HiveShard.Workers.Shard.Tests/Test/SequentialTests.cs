using HiveShard.Data;
using HiveShard.Deployments.DockerCompose;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.ShardWorker.Tests.Events;
using HiveShard.ShardWorker.Tests.Shards;
using HiveShard.Workers.Edge.Extensions;
using HiveShard.Workers.Shard.Extensions;
using InMemory;
using Xcepto;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;
using Xcepto.HiveShard.Scenario;

namespace HiveShard.Worker.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
[Ignore("InMemory Bus offsets are required for this behaviour. This is not yet implemented!")]
public class SequentialTests<T>
where T: IDeployment, new()
{
    private HiveShardIdentity _hiveShardIdentity;
    private string _hiveShardWorker;
    private HiveShardScenario _hiveShardScenario;

    [OneTimeSetUp]
    public void SetUp()
    {
        _hiveShardWorker = "SW1";
        _hiveShardIdentity = new HiveShardIdentity(new Chunk(0, 0), ShardType.From<EchoHiveShard>(), Guid.NewGuid());
        ServiceEnvironment environment = HiveShardFactory.Create<T>(builder => builder
            .SetGridSize(1)
            .ShardWorker(workerBuilder => workerBuilder
                .AddShard<EchoHiveShard>(_hiveShardIdentity.Chunk, _hiveShardIdentity.Id)
                .Identify(_hiveShardWorker)
            )
        );
        _hiveShardScenario = new HiveShardScenario(environment);
    }
    
    [Test]
    public async Task FirstSendTestEvent()
    {
        await HiveShardTest.GivenSequential(_hiveShardScenario, builder =>
        {
            var shardAdapter = builder.RegisterAdapter(new HiveShardShardAdapter(_hiveShardWorker, _hiveShardIdentity));

            shardAdapter.Action<IScopedShardTunnel>(x=>x.Send(new TestEvent(1)));
        });
    }
    
    [Test]
    public async Task ThenExpectResponseEvent()
    {
        await HiveShardTest.GivenSequential(_hiveShardScenario, builder =>
        {
            var shardAdapter = builder.RegisterAdapter(new HiveShardShardAdapter(_hiveShardWorker, _hiveShardIdentity));

            shardAdapter.ExpectEvent<TestEventResponse>(x => x.Number == 1);
        });
    }
}