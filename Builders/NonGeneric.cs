using Polly;
using Polly.Retry;

namespace Builders;

public static class NonGeneric
{
    public static void Example()
    {
        // Create an empty pipeline
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder().Build();

        // Builder configuration
        pipeline = new ResiliencePipelineBuilder()
        {
            Name = "my-pipeline",
            InstanceName = "my-instance",
            TimeProvider = TimeProvider.System,
            ContextPool = ResilienceContextPool.Shared,
        }
        .Build();


        // Configure strategies
        // 
        // Notice that Hedging and Fallback are not available for non-generic builders.
        pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(10))
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 5,
            })
            .Build();
    }
}
