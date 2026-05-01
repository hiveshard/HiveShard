using HiveShard.Data;
using HiveShard.Deployments.InMemory;
using HiveShard.Factory;
using HiveShard.Interface;
using HiveShard.Workers.Initialization.Tests.Config;
using HiveShard.Workers.Initialization.Tests.Events;
using HiveShard.Workers.Initialization.Tests.Initializer;
using HiveShard.Workers.Initialization.Tests.Shards;
using HiveShard.Workers.Initializer.Data;
using HiveShard.Workers.Initializer.Extensions;
using HiveShard.Workers.Shard.Extensions;
using HiveShard.Workers.Ticker.Extensions;
using Xcepto.HiveShard;
using Xcepto.HiveShard.Adapters;

namespace HiveShard.Workers.Initialization.Tests.Integration;

[TestFixture(typeof(InMemoryDeployment))]

public class InitializationDependencyTest<T>
where T : class, IDeployment, new()
{
    [Test]
    public async Task InitializerDependencyIsProperlyInjected()
    {
        SecretDependencyConfig config = new SecretDependencyConfig(7);
        var initializer = InitializerEmitterIdentity.From<DependentInitializer>();
        var environment = HiveShardFactory.Create<T>(builder => builder
            .Events(eventBuilder => eventBuilder
                .RegisterEvent<InitialDataEvent>(initializer)
            )
            .Initialize(initializationBuilder => initializationBuilder
                .AddInitializer<DependentInitializer>(initializer, initializerBuilder => initializerBuilder
                    .WithDependency(config)
                )
            )
            .TickerWorker(tickerWorker => tickerWorker
                .GlobalTicker()
                .Ticker<InitialDataEvent>()
            )
        );
        
        await HiveShardTest.Given(environment, builder =>
        {
            var fabricAdapter = builder.RegisterAdapter(new HiveShardFakeFabricAdapter(new EmitterIdentity("readonly")));
            
            fabricAdapter.FabricExpectation<DependencySecretEvent>(x => 
                x.Secret == config.Secret, 
                typeof(DependencySecretEvent).FullName!, 
                DependentInitializer.PublishedChunk.ToPartition(environment.GlobalChunkConfig)
            );
        });
    }
}