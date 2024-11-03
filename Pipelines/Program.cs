//
// Exercise 1: Apply resilience to data processing
//

using Pipelines.Utils;

var cancellationToken = CancellationToken.None;
var folders = new string[] { "file-1", "file-2", "file-3" };

foreach (var folder in folders)
{
    await ProcessingLibrary.ProcessFileAsync(folder, cancellationToken);
}

