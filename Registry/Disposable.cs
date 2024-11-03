using Polly;
using Polly.RateLimiting;
using Polly.Registry;
using System.Threading.RateLimiting;

namespace Registry;

// This example demonstrates the dispose feature of the registry.
internal static class Disposable
{
    public static void Example()
    {
        var registry = new ResiliencePipelineRegistry<string>();

        registry.TryAddBuilder("my-pipeline", (builder, context) =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(1));
            context.OnPipelineDisposed(() => Console.WriteLine("Pipeline disposed"));
        });

        // Access the pipeline
        var pipeline = registry.GetPipeline("my-pipeline");

        registry.Dispose();
    }


    public static void Example_With_Limiter()
    {
        var registry = new ResiliencePipelineRegistry<string>();

        registry.TryAddBuilder("my-pipeline", (builder, context) =>
        {
            var limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 10,
                SegmentsPerWindow = 5
            });

            builder.AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = args => limiter.AcquireAsync(1, args.Context.CancellationToken)
            }); 

            context.OnPipelineDisposed(() =>
            {
                limiter.Dispose();
                Console.WriteLine("Rate limiter disposed");
            });
        });

        // Access the pipeline
        var pipeline = registry.GetPipeline("my-pipeline");

        registry.Dispose();
    }
}