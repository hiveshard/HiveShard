using HiveShard.Data;
using HiveShard.Deployments.DockerCompose;
using HiveShard.Event;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.ShardWorker.Tests.Events;
using HiveShard.ShardWorker.Tests.Shards;
using HiveShard.Workers.Shard.Extensions;
using InMemory;
using Xcepto;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Worker.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
public class ShardTests<T>
where T: IDeployment, new()
{
    [Test]
    public async Task EchoShardResponseWithNumber()
    {
        var chunk = new Chunk(0, 0);
        var environment = HiveShardFactory.Create<T>(builder => builder
            .SetGridSize(1)
            .ShardWorker(x => x
                .AddShard<EchoHiveShard>(chunk, Guid.NewGuid())
            )
        );
        await HiveShardTest.Given(environment, builder =>
        {
            var simpleFabric = builder.RegisterAdapter(new HiveShardFakeFabricAdapter());

            // arrange
            TopicPartition topicPartition = new TopicPartition(typeof(TestEvent).FullName!, chunk);
            
            // act
            simpleFabric.FabricAction(x => x.Register<CompletedTick>("completed-ticks", e => { }));
            simpleFabric.FabricAction(x => x.Send(topicPartition.Topic, topicPartition.Chunk, new TestEvent(7)));
            simpleFabric.FabricAction(x => x.Send("ticks", new Tick(1, 0, [new TopicPartitionOffset(topicPartition.Topic, topicPartition.Chunk, 1)], DateTime.Now)));
            
            // assert
            simpleFabric.FabricExpectation<TestEventResponse>(x => x.Number == 7, typeof(TestEventResponse).FullName);
        });
    }
}