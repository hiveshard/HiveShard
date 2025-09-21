using System.Threading.Tasks;
using HiveShard.Data;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.Interfaces;
using Xcepto.Provider;

namespace Xcepto.HiveShard.Scenario
{
    public class GenericHiveShardScenario: XceptoScenario
    {
        private ServiceEnvironment _environment;

        public GenericHiveShardScenario(ServiceEnvironment environment)
        {
            _environment = environment;
        }

        protected override Task<IServiceCollection> Setup()
        {
            return Task.FromResult(_environment
                .GetServices()
                .AddSingleton<ServiceEnvironment>(_environment)
                .AddSingleton<ILoggingProvider, XceptoBasicLoggingProvider>());
        }
    }
}