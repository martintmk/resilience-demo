using Polly;

namespace Pipelines;

// Empty pipelines do nothing.
// They only execute the provided callback without any additional logic.
// Usefull for testing.
internal static class EmptyPipeline
{
    public static void Example()
    {
        ResiliencePipeline empty = ResiliencePipeline.Empty;

        ResiliencePipeline<string> empty_generic = ResiliencePipeline<string>.Empty;
    }
}
