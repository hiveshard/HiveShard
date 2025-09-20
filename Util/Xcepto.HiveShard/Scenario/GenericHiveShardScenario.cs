using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.Scenario
{
    public class GenericHiveShardScenario: XceptoScenario
    {
        private HiveShardEnvironment _environment;

        public GenericHiveShardScenario(HiveShardEnvironment environment)
        {
            _environment = environment;
        }

        protected override Task<IServiceCollection> Setup()
        {
            return Task.FromResult(_environment.GetServices());
        }
    }
}