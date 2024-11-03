using Exercises.Utils;
using Polly;

namespace Exercises;

//
// Exercise 1: Apply resilience to data processing
//
internal class Exercise1
{
    ResiliencePipeline resiliencePipeline = ResiliencePipeline.Empty; 

    public async Task Run(IEnumerable<string> files, CancellationToken cancellationToken)
    {
        foreach (var file in files)
        {
            await ProcessingLibrary.ProcessFileAsync(file, cancellationToken);
        }
    }
}
