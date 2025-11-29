using System;
using System.Threading.Tasks;
using HiveShard.Builder;
using HiveShard.Data;
using Xcepto.HiveShard.Scenario;

namespace Xcepto.HiveShard
{
    public class HiveShardTest
    {
        public static async Task Given(ServiceEnvironment environment, Action<TransitionBuilder> xceptoBuilder)
        {
            await XceptoTest.Given(new HiveShardScenario(environment), xceptoBuilder);
        }
        
        public static async Task GivenSequential(HiveShardScenario scenario, Action<TransitionBuilder> xceptoBuilder)
        {
            await XceptoTest.Given(scenario, xceptoBuilder);
        }
    }
}