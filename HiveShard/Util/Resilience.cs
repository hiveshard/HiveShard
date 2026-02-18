using System;
using System.Threading;
using System.Threading.Tasks;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using Polly;
using Polly.Retry;

namespace HiveShard.Util
{
    public static class Resilience
    {
        private static ResiliencePipeline Pipeline(IHiveShardTelemetry telemetry)
        {
            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions()
                {
                    MaxRetryAttempts = 5,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromSeconds(1),
                    OnRetry = args =>
                    {
                        var exception = args.Outcome.Exception;
                        telemetry.LogWarning($"[RETRY] Attempt {args.AttemptNumber}, Exception: {exception?.Message}, StackTrace: {exception?.StackTrace}");
                        return new ValueTask();
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(5))
                .Build();
        }
            


        public static async Task Retry(Func<CancellationToken, Task> action, string context, CancellationToken ctx, IHiveShardTelemetry telemetry)
        {
            int attempt = 0;
            await Pipeline(telemetry).ExecuteAsync(async token =>
                {
                    attempt += 1;
                    if (attempt > 1)
                        telemetry.LogWarning($"{context} retry attempt #{attempt-1}");
                    await action(token);
                }, 
                ctx);
        }
    }
}