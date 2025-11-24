using HiveShard.Client;
using HiveShard.Client.Interface;
using HiveShard.Config;
using HiveShard.Fabric.Client;
using HiveShard.Fabric.Edge;
using HiveShard.Fabrics.InMemory;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using Microsoft.Extensions.DependencyInjection;
using Xcepto;
using Xcepto.HiveShard.Providers;
using Xcepto.Interfaces;

namespace HiveShard.Edge.Tests.Scenario;

public class InMemoryScenario: XceptoScenario
{
    protected override Task<IServiceCollection> Setup() 
        => Task.FromResult<IServiceCollection>(
            new ServiceCollection()
                .AddSingleton<ILoggingProvider, LoggingProvider>()
                .AddSingleton<IDebugLoggingProvider, LoggingProvider>()
                .AddSingleton<CancellationProvider>()
                .AddSingleton<ICancellationProvider>(x => x.GetRequiredService<CancellationProvider>())
                .AddSingleton<InMemoryEdgeFabric>()
                .AddSingleton<IEdgeTunnelClientEndpoint>(x => x.GetRequiredService<InMemoryEdgeFabric>())
                .AddSingleton<IEdgeTunnelServerEndpoint>(x => x.GetRequiredService<InMemoryEdgeFabric>())
                .AddSingleton<ClientTunnel>()
                .AddSingleton<IClientTunnel>(x => x.GetRequiredService<ClientTunnel>())
                .AddSingleton<HiveShard.Data.HiveShardClient>(new HiveShard.Data.HiveShardClient("test client"))
                .AddSingleton<IAddressProvider, EdgeIdentityProvider>()
                .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "test identity"))
                .AddSingleton<IEdgeTunnel, EdgeTunnel>()
        );
}