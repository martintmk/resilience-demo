using Exercises.Utils;
using Polly;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;
using System.Diagnostics;

namespace Exercises;

//
// Exercise 4: Add telemetry to the pipeline
//
internal class Exercise4
{
    private readonly ResiliencePipelineRegistry<string> registry;

    public Exercise4(ResiliencePipelineRegistry<string> registry)
    {
        registry.TryAddBuilder<ProcessingStatus>("file-pipeline", (builder, context) =>
        {
            builder.AddRetry(new RetryStrategyOptions<ProcessingStatus>
            {
                Delay = TimeSpan.Zero,
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: InvalidOperationException } => PredicateResult.True(),
                    { Result: ProcessingStatus.Error } => PredicateResult.True(),
                    _ => PredicateResult.False(),
                }
            });

            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromMilliseconds(800),
            });
        });

        this.registry = registry;
    }

    public async Task Run(IEnumerable<string> files, CancellationToken cancellationToken)
    {
        foreach (var file in files)
        {
            var watch = Stopwatch.StartNew();

            Outcome<ProcessingStatus> result = await ProcessFile(file, cancellationToken);

            if (result.Exception is { } error)
            {
                HandleException(file, error, watch.Elapsed);
            }
            else
            {
                HandleResult(file, result.Result, watch.Elapsed);
            }
        }
    }

    private async Task<Outcome<ProcessingStatus>> ProcessFile(string file, CancellationToken cancellationToken)
    {
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

        return await registry.GetPipeline<ProcessingStatus>("file-pipeline").ExecuteOutcomeAsync(
            static async (context, file) =>
            {
                try
                {
                    return Outcome.FromResult(await ProcessingLibrary.ProcessFileAsync(file, context.CancellationToken));
                }
                catch (Exception e)
                {
                    return Outcome.FromException<ProcessingStatus>(e);
                }
            },
            context,
            file);
    }

    private void HandleResult(string file, ProcessingStatus status, TimeSpan elapsed)
    {
        Console.WriteLine($"File: '{file}', Status: '{status}', Elapsed: {elapsed.TotalMilliseconds}ms");
    }

    private void HandleException(string file, Exception e, TimeSpan elapsed)
    {
        Console.WriteLine($"File: '{file}', Error: '{e.GetType().Name}', Elapsed: {elapsed.TotalMilliseconds}ms");
    }
}
