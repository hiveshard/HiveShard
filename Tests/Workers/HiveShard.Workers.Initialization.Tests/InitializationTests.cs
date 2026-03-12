using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Ticker.Data;
using HiveShard.Workers.Initialization.Tests.Events;
using HiveShard.Workers.Initialization.Tests.Initializer;
using HiveShard.Workers.Initialization.Tests.Shards;
using HiveShard.Workers.Initializer.Data;
using HiveShard.Workers.Initializer.Extensions;
using HiveShard.Workers.Shard.Extensions;
using HiveShard.Workers.Ticker.Extensions;
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
        Guid shardWorker = Guid.NewGuid();
        var shard = new HiveShardIdentity(new Chunk(0, 0), ShardType.From<TestShard>(), Guid.NewGuid());
        var initializer = InitializerEmitterIdentity.From<TestShardInitializer>();
        var environment = HiveShardFactory.Create<T>(builder => builder
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<InitialDataEvent>(initializer)
                .RegisterEvent<InitialDataResponse>(shard)
            )
            .ShardWorker(workerBuilder => workerBuilder
                .Identify(shardWorker)
                .AddShard(shard)
            )
            .Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<TestShardInitializer>(initializer)
            )
            .TickerWorker(tickerWorker => tickerWorker
                .GlobalTicker()
                .Ticker<InitialDataEvent>()
                .Ticker<InitialDataResponse>()
            )
        );

        await HiveShardTest.Given(environment, builder =>
        {
            var shardAdapter = builder.RegisterAdapter(new HiveShardShardAdapter(shardWorker, shard));

            shardAdapter.Except<TestShard>(x=> 
                x.ReceivedIncrements == TestShardInitializer.Increments.Sum());
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