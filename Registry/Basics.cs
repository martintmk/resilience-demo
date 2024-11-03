using Polly;
using Polly.Registry;

namespace Registry;

internal static class Basics
{
    public static void Example()
    {
        // Registry with default options
        var registry = new ResiliencePipelineRegistry<string>();

        // Create or retrieve cached non-generic pipeline
        ResiliencePipeline pipeline = registry.GetOrAddPipeline(
            "pipeline-1", 
            builder => builder.AddTimeout(TimeSpan.FromMilliseconds(10)));

        // Create or retrieve cached generic pipeline
        ResiliencePipeline<string> pipeline_generic = registry.GetOrAddPipeline<string>(
            "pipeline-1", 
            builder => builder.AddTimeout(TimeSpan.FromMilliseconds(10)));

        // Pipelines can be registered eagerly using builders
        registry.TryAddBuilder(
            "pipeline-2",
            (builder, context) => builder.AddTimeout(TimeSpan.FromMilliseconds(10)));

        registry.TryAddBuilder<string>(
            "pipeline-2",
            (builder, context) => builder.AddTimeout(TimeSpan.FromMilliseconds(10)));

        // Read-only access to the registry
        ResiliencePipelineProvider<string> provider = registry;

        provider.GetPipeline("pipeline-2");
        provider.GetPipeline<string>("pipeline-2");

        // Not found
        try
        {
            provider.GetPipeline("pipeline-3");
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Pipeline not found");
        }


        provider.TryGetPipeline("not-existing", out var pipeline);
    }
}