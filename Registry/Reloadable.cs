using Polly;
using Polly.Registry;

namespace Registry;

// This example demonstrates the dispose feature of the registry.
internal static class Reloadable
{
    public static void Example()
    {
        var registry = new ResiliencePipelineRegistry<string>();
        var timeout = TimeSpan.FromSeconds(1);
        var reloadSource = new CancellationTokenSource();

        registry.TryAddBuilder("my-pipeline", (builder, context) =>
        {
            Console.WriteLine("Creatinga pipeline with a timeout of {0}s", timeout.TotalSeconds);

            builder.AddTimeout(timeout);

            context.AddReloadToken(reloadSource.Token);
            context.OnPipelineDisposed(() => Console.WriteLine("Pipeline disposed"));
        });

        // Access the pipeline
        var pipeline = registry.GetPipeline("my-pipeline");

        // Trigger the reload
        timeout = TimeSpan.FromSeconds(2);
        reloadSource.Cancel();

        // The pipeline is alive even after reload!

        registry.Dispose();
    }
}