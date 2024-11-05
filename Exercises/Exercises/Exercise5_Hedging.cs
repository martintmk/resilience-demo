using Exercises.Utils;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;
using System.Diagnostics;

namespace Exercises;

//
// Exercise 5: Use hedging to speed up the processing
//
internal class Exercise5
{
    private readonly ResiliencePipelineRegistry<string> registry;

    public Exercise5(ResiliencePipelineRegistry<string> registry, ILoggerFactory loggerFactory)
    {
        registry.TryAddBuilder<ProcessingStatus>("file-pipeline", (builder, context) =>
        {
            builder.AddRetry(new RetryStrategyOptions<ProcessingStatus>
            {
                Delay = TimeSpan.FromMilliseconds(10),
                MaxRetryAttempts = 5,
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: InvalidOperationException } => PredicateResult.True(),
                    { Exception: TimeoutRejectedException } => PredicateResult.True(),
                    { Result: ProcessingStatus.Error } => PredicateResult.True(),
                    _ => PredicateResult.False(),
                }
            });

            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromMilliseconds(800),
            });


            builder.ConfigureTelemetry(loggerFactory);
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

        try
        {
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
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
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
