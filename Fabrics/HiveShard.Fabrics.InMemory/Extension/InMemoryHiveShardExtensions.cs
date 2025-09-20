using HiveShard.Fabric.Client;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.Builder;

namespace HiveShard.Fabrics.InMemory.Extension
{
    public static class InMemoryHiveShardExtensions
    {
        public static HiveShardBuilder AddInMemoryClient(this HiveShardBuilder builder)
        {
            var environment = builder.Build();
            var services = environment.GetServices();
            services.AddSingleton<IEdgeTunnelClientEndpoint>();
            return new HiveShardBuilder(environment);
        }
    }
}