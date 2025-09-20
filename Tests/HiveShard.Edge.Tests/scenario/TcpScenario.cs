using HiveShard.Client;
using HiveShard.Client.Interface;
using HiveShard.Config;
using HiveShard.Edge.Tests.provider;
using HiveShard.Fabric.Client;
using HiveShard.Fabric.Edge;
using HiveShard.Fabrics.Tcp;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Provider;
using HiveShard.Provider.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using Microsoft.Extensions.DependencyInjection;
using Xcepto;
using Xcepto.HiveShard.Providers;
using Xcepto.Interfaces;

namespace HiveShard.Edge.Tests.scenario;

public class TcpScenario: XceptoScenario
{
    protected override Task<IServiceCollection> Setup()
    {
        var collection = new ServiceCollection()
            .AddSingleton<ITickRepository, TickRepository>()
            .AddSingleton<IHiveShardSimpleLoggingProvider, LoggingProvider>()
            .AddSingleton<ITelemetryProvider, SimpleTelemetryProvider>()
            .AddSingleton<IFabricLoggingProvider, FabricLoggingProvider>()
            .AddSingleton<ILoggingProvider, LoggingProvider>()
            .AddSingleton<INetworkConfiguration, RandomNetworkConfigurationProvider>()
            .AddSingleton<ISerializer, NewtonsoftSerializer>()
            .AddSingleton<CancellationProvider>()
            .AddSingleton<ICancellationProvider>(x => x.GetRequiredService<CancellationProvider>())
            .AddSingleton<IEdgeTunnelClientEndpoint, ClientTcpFabric>()
            .AddSingleton<IEdgeTunnelServerEndpoint, EdgeTcpFabric>()
            .AddSingleton<ClientTunnel>()
            .AddSingleton<IClientTunnel>(x => x.GetRequiredService<ClientTunnel>())
            .AddSingleton<Data.Client>(new Data.Client("test client"))
            .AddSingleton<IAddressProvider, EdgeIdentityProvider>()
            .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "test identity"))
            .AddSingleton<IEdgeTunnel, EdgeTunnel>();
        return Task.FromResult(collection);
    }
}