using System;
using HiveShard.Config;
using HiveShard.Fabrics.InMemory.Providers;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;

namespace HiveShard.Fabrics.InMemory.Builders;

public class InMemorySimpleFabricBuilder
{
    public InMemorySimpleFabric Build()
    {
        ITelemetryProvider telemetryProvider = new SimpleTelemetryProvider(new SimpleConsoleLoggingProvider());
        ITickRepository tickRepository = new TickRepository();
        IFabricLoggingProvider fabricLoggingProvider = new FabricLoggingProvider(telemetryProvider, tickRepository);
        IIdentityConfig identityConfig = new IdentityConfig(Guid.NewGuid(), "test");
        ICancellationProvider cancellationProvider = new CancellationProvider();
        return new InMemorySimpleFabric(fabricLoggingProvider, identityConfig, cancellationProvider);
    }
}