using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory.Builders;
using HiveShard.Repository;
using HiveShard.Telemetry.Console;
using HiveShard.Workers.Initialization.Tests.Data;
using HiveShard.Workers.Initialization.Tests.Events;
using HiveShard.Workers.Initialization.Tests.Initializer;
using HiveShard.Workers.Initialization.Tests.Util;
using HiveShard.Workers.Initializer;
using HiveShard.Workers.Initializer.Data;

namespace HiveShard.Workers.Initialization.Tests.Unit;

[TestFixture]
public class InitializationTunnelTests
{
    [Test]
    public void InitializerRespondsWithCompletedOnTick0()
    {
        InitializationTunnelTest<TestShardInitializer> test = InitializationTunnelTest<TestShardInitializer>.CreateTestInitializer();

        test.SendTick<InitialDataEvent>(0);

        var completedTick = test.FetchCompletedTopic<InitialDataEvent>(0, 1)
            .Select(x => x.Message.Payload)
            .Cast<CompletedTick>()
            .FirstOrDefault();
        Assert.That(completedTick, Is.Not.Null);
    }
}