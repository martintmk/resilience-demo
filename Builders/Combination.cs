using Polly;

namespace Builders;

// This example demonstrates how the builder cab be used to combine the pipelines.
public static class Combination
{
    public static void Example()
    {
        // Generic pipeline
        ResiliencePipeline pipeline1 = new ResiliencePipelineBuilder().Build();

        // Non-Generic pipeline
        ResiliencePipeline<string> pipeline2 = new ResiliencePipelineBuilder<string>().Build();

        // Combine non-generic pipelines
        ResiliencePipeline combinedPipeline1 = new ResiliencePipelineBuilder()
            .AddPipeline(pipeline1)
            .Build();

        // Combine generic pipelines
        ResiliencePipeline<string> combinedPipeline2 = new ResiliencePipelineBuilder<string>()
            .AddPipeline(pipeline1)
            .AddPipeline(pipeline2)
            .AddPipeline(combinedPipeline1)
            .Build();
    }
}
