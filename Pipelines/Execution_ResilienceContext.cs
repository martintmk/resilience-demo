using Polly;

namespace Pipelines;

// Demonstration of how pipelines can be executed.
internal static class Execution_ResilienceContext
{
    public static async Task Example(ResiliencePipeline pipeline, CancellationToken cancellationToken)
    {
        // Rent the context
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);

        // Set the request name
        context.Properties.Set(ResilienceProperties.RequestName, "my-request-id");

        try
        {
            await pipeline.ExecuteAsync(async context =>
            {
                // Retrieve the custom resilience property
                var requestName = context.Properties.GetValue(ResilienceProperties.RequestName, "empty");

                await Task.Delay(1000, context.CancellationToken);

                Console.WriteLine("Request Name: {0}", requestName);
            },
            context);
        }
        finally
        {
            // Return the context back to the pool after use
            ResilienceContextPool.Shared.Return(context);
        }
    }
}

public static class ResilienceProperties
{
    public static readonly ResiliencePropertyKey<string> RequestName = new("RequestName");
}
