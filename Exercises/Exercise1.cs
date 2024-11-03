using Exercises.Utils;
using Polly;
using System.Diagnostics;

namespace Exercises;

//
// Exercise 1: Use resilience pipeline to execute file processing
//
internal class Exercise1
{
    ResiliencePipeline<ProcessingStatus> resiliencePipeline = ResiliencePipeline<ProcessingStatus>.Empty;

    public async Task Run(IEnumerable<string> files, CancellationToken cancellationToken)
    {
        foreach (var file in files)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                ProcessingStatus result = await ProcessingLibrary.ProcessFileAsync(file, cancellationToken);
                HandleResult(file, result, watch.Elapsed);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                HandleException(file, e, watch.Elapsed);
            }
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