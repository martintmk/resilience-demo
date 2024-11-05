using Polly;
using Polly.Registry;
using Polly.Retry;
using Polly.Testing;
using Polly.Timeout;
using Xunit;

internal static class Composition
{
    public static void Example()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 5,
            })
            .AddTimeout(TimeSpan.FromSeconds(10))
            .Build();

        var descriptor = pipeline.GetPipelineDescriptor();

        // Assert the composition of the pipeline
        Assert.Equal(2, descriptor.Strategies.Count);

        var retryOptions = Assert.IsType<RetryStrategyOptions>(descriptor.Strategies[0].Options);
        Assert.Equal(5, retryOptions.MaxRetryAttempts);

        var timeoutOptions = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[1].Options);
        Assert.Equal(TimeSpan.FromSeconds(10), timeoutOptions.Timeout);
    }
}
