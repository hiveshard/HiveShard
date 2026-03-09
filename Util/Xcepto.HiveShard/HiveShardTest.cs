using System;
using System.Threading.Tasks;
using HiveShard.Data;
using Xcepto.Builder;
using Xcepto.Config;
using Xcepto.HiveShard.Scenario;

namespace Xcepto.HiveShard;

public class HiveShardTest
{
    private static TimeoutConfig DefaultTimeout => TimeoutConfig.FromSeconds(30);
    public static async Task Given(ServiceEnvironment environment, Action<TransitionBuilder> xceptoBuilder)
    {
        await XceptoTest.Given(new HiveShardScenario(environment), DefaultTimeout, xceptoBuilder);
    }

    public static async Task GivenSequential(HiveShardScenario scenario, Action<TransitionBuilder> xceptoBuilder)
    {
        await XceptoTest.Given(scenario, DefaultTimeout, xceptoBuilder);
    }
}