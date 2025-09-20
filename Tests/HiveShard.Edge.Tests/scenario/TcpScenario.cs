using HiveShard.Edge.Tests.provider;
using HiveShard.Fabric.Client;
using HiveShard.Fabric.Edge;
using HiveShard.Fabrics.Tcp;
using HiveShard.Interface;
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
            .AddSingleton<IEdgeTunnelServerEndpoint, EdgeTcpFabric>();
        return Task.FromResult(collection);
    }
}