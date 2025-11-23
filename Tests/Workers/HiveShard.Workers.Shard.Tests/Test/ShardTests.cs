using HiveShard.Data;
using HiveShard.Deployments.DockerCompose;
using HiveShard.Deployments.HiveShardPlatform;
using HiveShard.Event;
using HiveShard.ShardWorker.Tests.Events;
using HiveShard.ShardWorker.Tests.Shards;
using InMemory;
using Xcepto;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Worker.Tests.Test;

[TestFixture(typeof(InMemoryDeployment))]
[TestFixture(typeof(DockerComposeDeployment))]
[TestFixture(typeof(HiveShardPlatformDeployment))]
public class ShardTests<T>
where T: XceptoScenario, new()
{
    [Test]
    [Ignore("No consistency in failures")]
    public async Task EchoShardResponseWithNumber()
    {
        await XceptoTest.Given(new T(), builder =>
        {
            var worker = builder.RegisterAdapter(new WorkerXceptoAdapter());
            var simpleFabric = builder.RegisterAdapter(new HiveShardFakeFabricAdapter());

            // arrange
            worker.AddHiveShardStep<EchoHiveShard>();
            TopicPartition topicPartition = new TopicPartition(typeof(TestEvent).FullName, new Chunk(0, 0));
            
            // act
            simpleFabric.FabricAction(x => x.Register<CompletedTick>("completed-ticks", e => { }));
            simpleFabric.FabricAction(x => x.Send(topicPartition.Topic, topicPartition.Chunk, new TestEvent(7)));
            simpleFabric.FabricAction(x => x.Send("ticks", new Tick(1, 0, [new TopicPartitionOffset(topicPartition.Topic, topicPartition.Chunk, 1)], DateTime.Now)));
            
            // assert
            simpleFabric.FabricExpectation<TestEventResponse>(x => x.Number == 7, typeof(TestEventResponse).FullName);
        });
    }
}