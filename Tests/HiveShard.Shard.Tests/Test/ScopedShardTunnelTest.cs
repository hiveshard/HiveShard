using System.Collections.Concurrent;
using HiveShard.Config;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;
using HiveShard.Shard.Tests.Events;
using HiveShard.Shard.Tests.Shards;
using NUnit.Framework;
using Xcepto.HiveShard.Providers;

namespace HiveShard.Shard.Tests.Test;

[TestFixture]
public class ScopedShardTunnelTest
{
    [Test]
    public async Task Test()
    {
        var shardIdentity = new HiveShardIdentity(new Chunk(0, 0), new ShardType("test"));
        var loggingProvider = new LoggingProvider();
        var identityConfig = new IdentityConfig(Guid.NewGuid(), "test");
        var tickRepository = new TickRepository();
        var simpleTelemetryProvider = new SimpleTelemetryProvider(loggingProvider);
        var workerLoggingProvider = new WorkerLoggingProvider(simpleTelemetryProvider, tickRepository, identityConfig);
        var fabricLoggingProvider = new FabricLoggingProvider(simpleTelemetryProvider, tickRepository);
        ICancellationProvider cancellationProvider = new CancellationProvider();
        var inMemorySimpleFabric = new InMemorySimpleFabric(fabricLoggingProvider, identityConfig, cancellationProvider);
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        ScopedShardTunnel tunnel = new ScopedShardTunnel(shardIdentity, workerLoggingProvider, inMemorySimpleFabric, tickRepository);
        tunnel.Initialize(new TestShard(tunnel));

        // Arrange
        BlockingCollection<TestEventResponse> responses = new BlockingCollection<TestEventResponse>();
        inMemorySimpleFabric.Register<CompletedTick>("completed-ticks", x => { }); // ignore not consumed yet
        inMemorySimpleFabric.Register<TestEventResponse>(typeof(TestEventResponse).FullName!, e =>
        {
            Console.WriteLine("received response");
            responses.Add(e.Message);
        });
        Task tunnelStart = tunnel.Start(cancellationTokenSource.Token);
        await tunnel.WaitForReady();
        
        // Act
        var testEventPartition = new TopicPartition(typeof(TestEvent).FullName!, new Chunk(0, 0));
        await inMemorySimpleFabric.Send(testEventPartition.Topic, testEventPartition.Chunk , new TestEvent(7));
        await inMemorySimpleFabric.Send("ticks", new Tick(1, 0, [new TopicPartitionOffset(testEventPartition.Topic, testEventPartition.Chunk, 1)], DateTime.Now));

        
        // Assert
        var completed = await Task.WhenAny(tunnelStart, Task.Run(() =>
        {
            var eventResponse = responses.Take();
            Assert.That(eventResponse.Number == 7);
        }));
        if(completed.IsFaulted)
            throw completed.Exception;
    }
}