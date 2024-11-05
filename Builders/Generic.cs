using Polly;
using Polly.Hedging;

namespace Builders;

public static class Generic
{
    public static void Example()
    {
        // Create an empty pipeline
        ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>().Build();

        // Builder configuration
        pipeline = new ResiliencePipelineBuilder<string>()
        {
            Name = "my-pipeline",
            InstanceName = "my-instance",
            TimeProvider = TimeProvider.System,
            ContextPool = ResilienceContextPool.Shared,
        }
        .Build();


        // Configure strategies
        pipeline = new ResiliencePipelineBuilder<string>()
            .AddTimeout(TimeSpan.FromSeconds(10))
            .AddHedging(new HedgingStrategyOptions<string>()
            {
                MaxHedgedAttempts = 5,
            })
            .Build();
    }

}
