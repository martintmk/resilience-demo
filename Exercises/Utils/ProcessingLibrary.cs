using Polly;
using Polly.Simmy;

namespace Exercises.Utils;

public enum ProcessingStatus
{
    Success,
    Error
}

public interface IFileProcessor
{
    Task<ProcessingStatus> ProcessFileAsync(string file, CancellationToken cancellationToken);
}

public static class ProcessingLibrary
{
    public static readonly IFileProcessor MainProcessor = new ChaosProcessor(new FileProcessor(), 0.01, 0.02, 0.05);

    public static readonly IFileProcessor SecondaryProcessor = new FileProcessor();

    public static Task<ProcessingStatus> ProcessFileAsync(string file, CancellationToken cancellationToken)
    {
        return MainProcessor.ProcessFileAsync(file, cancellationToken);
    }

    public static string GetProcessorType(IFileProcessor processor)
    {
        return processor switch
        {
            _ when processor == MainProcessor => "main",
            _ when processor == SecondaryProcessor => "secondary",
            _ => "unknown"
        };
    }
}

public class FileProcessor : IFileProcessor
{
    public async Task<ProcessingStatus> ProcessFileAsync(string file, CancellationToken cancellationToken)
    {
        // imagine some IO-heavy processing of files inside the folder here
        await Task.Delay(3, cancellationToken);

        return ProcessingStatus.Success;
    }
}


public class ChaosProcessor : IFileProcessor
{
    private readonly IFileProcessor inner;
    private readonly ResiliencePipeline<ProcessingStatus> pipeline;

    public ChaosProcessor(IFileProcessor inner, double latencyRate, double resultRate, double exceptionRate)
    {
        pipeline = new ResiliencePipelineBuilder<ProcessingStatus>()
            .AddChaosLatency(latencyRate, TimeSpan.FromSeconds(5))
            .AddChaosOutcome(resultRate, () => ProcessingStatus.Error)
            .AddChaosFault(exceptionRate, () => new InvalidOperationException())
            .Build();

        this.inner = inner;
    }

    public async Task<ProcessingStatus> ProcessFileAsync(string file, CancellationToken cancellationToken)
    {
        return await pipeline.ExecuteAsync(static async (pair, token) =>
        {
            return await pair.inner.ProcessFileAsync(pair.file, token);
        }, 
        (inner, file), 
        cancellationToken);

    }
}



