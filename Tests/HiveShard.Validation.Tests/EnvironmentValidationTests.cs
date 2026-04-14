using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Exceptions;
using HiveShard.Factory;
using HiveShard.Validation.Tests.Events;
using HiveShard.Validation.Tests.Initializer;
using HiveShard.Validation.Tests.Shards;
using HiveShard.Workers.Initializer.Data;
using HiveShard.Workers.Initializer.Extensions;
using HiveShard.Workers.Shard.Extensions;

namespace HiveShard.Validation.Tests;

[TestFixture]
public class EnvironmentValidationTests
{
    [Test]
    public void EnvironmentsWithoutEvents_ShouldFail()
    {
        Assert.That(() =>
        {
            HiveShardFactory.Create<InMemoryDeployment>(builder => builder
                .ShardWorker(shardWorker => shardWorker
                    .AddShard<NoEventShard>(new Chunk(0,0), Guid.NewGuid())
                )
            );
        }, Throws.TypeOf<HiveShardValidationException>());
        
    }
    
    [Test]
    public void ShardsWithoutEmittingEvents_ShouldFail()
    {
        var chunk = new Chunk(0, 0);
        HiveShardIdentity consumeOnlyEvent = new HiveShardIdentity(chunk, ShardType.From<OnlyConsumeShard>(), Guid.NewGuid());
        InitializerEmitterIdentity plainInitializer = new InitializerEmitterIdentity(new EmitterIdentity("initializer"));
        Assert.That(() =>
        {
            HiveShardFactory.Create<InMemoryDeployment>(builder => builder
                .Events(eventBuilder => eventBuilder
                    .RegisterEvent<InputOnlyInitEvent>(consumeOnlyEvent)
                )
                .Initialize(initializationBuilder => initializationBuilder
                    .AddInitializer<PlainInitializer>(plainInitializer)
                )
                .ShardWorker(shardWorker => shardWorker
                    .AddShard(consumeOnlyEvent)
                )
            );
        }, Throws.TypeOf<HiveShardValidationException>());
        
    }
}