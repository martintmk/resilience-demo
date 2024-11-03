namespace Exercises.Utils;

public static class ProcessingLibrary
{
    public static readonly FileProcessor MainProcessor = new FileProcessor();

    public static readonly FileProcessor SecondaryProcessor = new FileProcessor();

    public static Task<FileOverview> ProcessFileAsync(string file, CancellationToken cancellationToken)
    {
        return MainProcessor.ProcessFileAsync(file, cancellationToken);
    }
}


public record FileOverview(string Overview);

public class FileProcessor
{
    public async Task<FileOverview> ProcessFileAsync(string file, CancellationToken cancellationToken)
    {
        await Task.Yield();

        // imagine some IO-heavy processing of files inside the folder here

        return new FileOverview("Overview");
    }
}
