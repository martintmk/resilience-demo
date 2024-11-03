using Exercises.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Polly;
using Polly.Hedging;
using Polly.Timeout;
using System.Diagnostics;

namespace Exercises;

//
// Exercise 8: Add the "file.processor.type" dimension to all metrics
//
internal class Exercise8
{
    private readonly ResiliencePipeline<ProcessingStatus> pipeline;

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddResiliencePipeline<string, ProcessingStatus>("file-pipeline", builder =>
        {
            builder.AddHedging(new HedgingStrategyOptions<ProcessingStatus>
            {
                Delay = TimeSpan.FromMilliseconds(100),
                MaxHedgedAttempts = 5,
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: InvalidOperationException } => PredicateResult.True(),
                    { Result: ProcessingStatus.Error } => PredicateResult.True(),
                    _ => PredicateResult.False(),
                },
                OnHedging = args =>
                {
                    args.ActionContext.Properties.Set(Properties.FileProcessor, ProcessingLibrary.SecondaryProcessor);
                    return default;
                }
            });

            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromMilliseconds(300),
            });
        });

        services.TryAddSingleton<Exercise8>();
    }

    public Exercise8([FromKeyedServices("file-pipeline")]ResiliencePipeline<ProcessingStatus> pipeline)
    {
        this.pipeline = pipeline;
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
        context.Properties.Set(Properties.FileProcessor, ProcessingLibrary.MainProcessor);

        try
        {
            return await pipeline.ExecuteOutcomeAsync(
                static async (context, file) =>
                {
                    var processor = context.Properties.GetValue(Properties.FileProcessor, ProcessingLibrary.MainProcessor);

                    try
                    {
                        return Outcome.FromResult(await processor.ProcessFileAsync(file, context.CancellationToken));
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


    class Properties
    {
        public static readonly ResiliencePropertyKey<IFileProcessor> FileProcessor = new("FileProcessor");
    }
}
