using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Worker.Data;
using HiveShard.Worker.Tests.Events;
using HiveShard.Worker.Tests.Scenarios;
using HiveShard.Worker.Tests.Shards;
using Xcepto;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Worker.Tests.Test;

[TestFixture(typeof(InMemoryScenario))]
[TestFixture(typeof(KafkaScenario))]
public class ShardTests<T>
where T: XceptoScenario, new()
{
    [Test]
    public async Task EchoShardResponseWithNumber()
    {
        await XceptoTest.Given(new T(), TimeSpan.FromSeconds(20), builder =>
        {
            var worker = builder.RegisterAdapter(new WorkerXceptoAdapter(new WorkerConfig(1)));
            var simpleFabric = builder.RegisterAdapter(new SimpleFabricXceptoAdapter());

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