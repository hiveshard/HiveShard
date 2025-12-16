using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Workers.Initialization.Tests.Initializer;
using HiveShard.Workers.Initialization.Tests.Repositories;
using HiveShard.Workers.Initialization.Tests.Shards;
using HiveShard.Workers.Initializer.Extensions;
using HiveShard.Workers.Shard.Extensions;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Workers.Initialization.Tests;

[TestFixture(typeof(InMemoryDeployment))]
public class InitializationTests<T>
where T: class, IDeployment, new()
{
    [Test]
    public async Task TestInitializerCausedServiceToWriteToRepository()
    {
        string shardWorker = "SW1";
        List<int> increments = new List<int>()
        {
            2, 5, 6, 12, 25
        };
        var shard = new HiveShardIdentity(new Chunk(0, 0), ShardType.From<TestShard>(), Guid.NewGuid());
        var environment = HiveShardFactory.Create<T>(builder => builder
            .ShardWorker(workerBuilder => workerBuilder
                .Identify(shardWorker)
                .AddShard(shard)
            )
            .Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<TestShardInitializer>()
            )
        );

        await HiveShardTest.Given(environment, builder =>
        {
            var shardAdapter = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorker, shard));

            shardAdapter.Except<TestShard>(x=> x.ReceivedIncrements == increments.Sum());
        });
    }


    [Test]
    public void TestTwoInitializersNotPossible()
    {
        Assert.CatchAsync<InvalidOperationException>(async () =>
        {
            var environment = HiveShardFactory.Create<T>(builder => builder
                .Initialize(initializationBuilder => initializationBuilder)
                .Initialize(initializationBuilder => initializationBuilder)
            );
            await HiveShardTest.Given(environment, _ => {});
        });
    }
}