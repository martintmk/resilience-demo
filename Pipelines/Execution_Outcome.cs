using Polly;

namespace Pipelines;

internal static class Execution_Outcome
{
    public static async Task Example(ResiliencePipeline pipeline, CancellationToken cancellationToken)
    {
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);

        // For maximum performance and flexibility, the ExecuteOutcomeAsync is recommended.
        Outcome<string> outcome = await pipeline.ExecuteOutcomeAsync(
            async (context, state) =>
            {
                await Task.Delay(1000, context.CancellationToken);

                return Outcome.FromResult("Hello World!");
            },
            context,
            "my-state");


        ResilienceContextPool.Shared.Return(context);
    }
}
