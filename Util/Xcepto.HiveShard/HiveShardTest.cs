using System;
using System.Threading.Tasks;
using HiveShard.Builder;
using HiveShard.Data;
using Xcepto.HiveShard.Scenario;

namespace Xcepto.HiveShard
{
    public class HiveShardTest
    {
        public static async Task RunAsync(ServiceEnvironment environment, Action<TransitionBuilder> xceptoBuilder)
        {
            await XceptoTest.Given(new HiveShardScenario(environment), xceptoBuilder);
        }
    }
}