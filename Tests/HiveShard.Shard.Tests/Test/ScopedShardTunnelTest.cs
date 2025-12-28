using System.Collections.Concurrent;
using HiveShard.Config;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Providers;
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
        var onlyChunk = new Chunk(0, 0);
        var shardIdentity = new HiveShardIdentity(onlyChunk, ShardType.From<TestShard>(), Guid.NewGuid());
        var loggingProvider = new LoggingProvider();
        var identityConfig = new IdentityConfig(Guid.NewGuid(), "test");
        var tickRepository = new TickRepository();
        var simpleTelemetryProvider = new SimpleTelemetryProvider(loggingProvider);
        var workerLoggingProvider = new WorkerLoggingProvider(simpleTelemetryProvider, tickRepository, identityConfig);
        var fabricLoggingProvider = new FabricLoggingProvider(simpleTelemetryProvider, tickRepository);
        ICancellationProvider cancellationProvider = new CancellationProvider();
        var globalChunkConfig = new GlobalChunkConfig(onlyChunk, onlyChunk);
        var inMemorySimpleFabric = new InMemorySimpleFabric(fabricLoggingProvider, identityConfig, globalChunkConfig);
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        ScopedShardTunnel tunnel = new ScopedShardTunnel(shardIdentity, workerLoggingProvider, inMemorySimpleFabric, tickRepository, cancellationProvider, globalChunkConfig);
        tunnel.Initialize(new TestShard(tunnel));

        // Arrange
        BlockingCollection<TestEventResponse> responses = new BlockingCollection<TestEventResponse>();
        inMemorySimpleFabric.Register<CompletedTick>("completed-ticks", x => { }); // ignore not consumed yet
        inMemorySimpleFabric.Register<TestEventResponse>(typeof(TestEventResponse).FullName!, e =>
        {
            Console.WriteLine("received response");
            responses.Add(e.Message);
        });
        Task tunnelStart = tunnel.Start();
        await tunnel.WaitForReady();
        
        // Act
        var testEventPartition = new TopicPartition(typeof(TestEvent).FullName!, onlyChunk);
        await inMemorySimpleFabric.Send(testEventPartition.Topic, testEventPartition.Chunk , new TestEvent(7));
        await inMemorySimpleFabric.Send("ticks", new Tick(1, 0, [new TopicPartitionOffset(testEventPartition.Topic, testEventPartition.Chunk, 1)], DateTime.Now));

        
        // Assert
        var completed = await Task.WhenAny(tunnelStart, Task.Run(() =>
        {
            Assert.That(responses.TryTake(out var eventResponse, TimeSpan.FromSeconds(5)), Is.True);
            Assert.That(eventResponse.Number, Is.EqualTo(7));
        }));
        if(completed.IsFaulted)
            throw completed.Exception;
    }
}