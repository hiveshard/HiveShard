using HiveShard.Data;
using HiveShard.Deployments.DockerCompose;
using HiveShard.Deployments.HiveShardPlatform;
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

namespace HiveShard.Worker.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
[TestFixture(typeof(DockerComposeDeployment))]
[TestFixture(typeof(HiveShardPlatformDeployment))]
public class SequentialTests<T>
where T: IDeployment, new()
{
    private ServiceEnvironment _environment;
    private ShardType _shardType;
    private Chunk _chunk;

    [OneTimeSetUp]
    public void SetUp()
    {
        _environment = HiveShardFactory.Create<T>(builder => builder
            .SetGridSize(1)
            .ShardWorker(workerBuilder => workerBuilder
                .AddShard<EchoHiveShard>()
            )
        );
        _shardType = ShardType.From<EchoHiveShard>();
        _chunk = new Chunk(0,0);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        
    }
    
    
    [Test]
    public async Task FirstSendTestEvent()
    {
        await HiveShardTest.RunAsync(_environment, builder =>
        {
            var shardAdapter = builder.RegisterAdapter(new HiveShardShardAdapter(_shardType, _chunk));

            shardAdapter.Action(x=>x.Send(new TestEvent(1)));
        });
    }
    
    [Test]
    public async Task ThenExpectResponseEvent()
    {
        await HiveShardTest.RunAsync(_environment, builder =>
        {
            var shardAdapter = builder.RegisterAdapter(new HiveShardShardAdapter(_shardType, _chunk));

            shardAdapter.Expect<TestEventResponse>(x => x.Number == 1);
        });
    }
}