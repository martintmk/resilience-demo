using Exercises.Utils;
using Polly;
using System.Diagnostics;

namespace Exercises;

//
// Exercise 2: Use pipeline with timeout and retries
//
class Exercise2
{
    ResiliencePipeline<ProcessingStatus> resiliencePipeline = ResiliencePipeline<ProcessingStatus>.Empty;

    public async Task Run(IEnumerable<string> files, CancellationToken cancellationToken)
    {
        foreach (var file in files)
        {
            var watch = Stopwatch.StartNew();

            Outcome<ProcessingStatus> result = await ProcessFile(file, cancellationToken);

            if (result.Exception is { } error )
            {
                HandleException(file, error, watch.Elapsed);
            }
            else
            {
                HandleResult(file, result.Result, watch.Elapsed);
            }
        }
    }

    async Task<Outcome<ProcessingStatus>> ProcessFile(string file, CancellationToken cancellationToken)
    {
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

        try
        {
            return await resiliencePipeline.ExecuteOutcomeAsync(
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

    void HandleResult(string file, ProcessingStatus status, TimeSpan elapsed)
    {
        Console.WriteLine($"File: '{file}', Status: '{status}', Elapsed: {elapsed.TotalMilliseconds}ms");
    }

    void HandleException(string file, Exception e, TimeSpan elapsed)
    {
        Console.WriteLine($"File: '{file}', Error: '{e.GetType().Name}', Elapsed: {elapsed.TotalMilliseconds}ms");
    }
}
