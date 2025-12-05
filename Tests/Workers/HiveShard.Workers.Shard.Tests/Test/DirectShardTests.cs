using HiveShard.Config;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabric.Ticker;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;
using HiveShard.Shard;
using HiveShard.ShardWorker.Tests.Events;
using HiveShard.ShardWorker.Tests.Shards;
using InMemory.Providers;

namespace HiveShard.Worker.Tests.Test;

[TestFixture]
public class DirectShardTests
{
    [Test]
    public async Task TestShardDirectly()
    {
        var partition = new Chunk(0,0);
        HiveShardIdentity identity = new HiveShardIdentity(partition, ShardType.From<EchoHiveShard>(), Guid.NewGuid());
        SimpleLoggingProvider loggingProvider = new SimpleLoggingProvider();
        ICancellationProvider cancellationProvider = new CancellationProvider();
        var tickRepository = new TickRepository();
        var fabricLoggingProvider = new FabricLoggingProvider(new SimpleTelemetryProvider(loggingProvider), tickRepository);
        var identityConfig = new IdentityConfig(Guid.NewGuid(), "test");
        ISimpleFabric simpleFabric = new InMemorySimpleFabric(fabricLoggingProvider, identityConfig, cancellationProvider);
        IScopedShardTunnel tunnel = new ScopedShardTunnel(identity, loggingProvider, simpleFabric, tickRepository, cancellationProvider);
        EchoHiveShard shard = new EchoHiveShard(tunnel);
        
        tunnel.Initialize(shard);
        shard.Initialize();
        _ = tunnel.Start();


        bool responseArrived = false;
        simpleFabric.Register<TestEventResponse>(typeof(TestEventResponse).FullName!, response =>
        {
            var testEventResponse = response.Message;
            Assert.That(testEventResponse.Number, Is.EqualTo(5));
            responseArrived = true;
        });
        simpleFabric.Register<CompletedTick>("completed-ticks", partition, consumption => { });
        
        await tunnel.Send(new TestEvent(5));
        await simpleFabric.Send("ticks", partition, new Tick(1, 0, new List<TopicPartitionOffset>()
        {
            new TopicPartitionOffset(typeof(TestEvent).FullName!, partition, 1)
        }, DateTime.Now));

        await Task.Delay(TimeSpan.FromSeconds(5));
        
        Assert.That(responseArrived, Is.EqualTo(true));
    }
}