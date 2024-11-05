using Polly;
using Polly.Registry;
using Polly.Telemetry;
using Polly.Timeout;

namespace Registry;

internal static class Configuration
{
    public static async Task Example(CancellationToken cancellationToken)
    {
        // Registry with custom options
        var registry = new ResiliencePipelineRegistry<string>(new ResiliencePipelineRegistryOptions<string>
        {
            BuilderComparer = StringComparer.OrdinalIgnoreCase,
            BuilderNameFormatter = name=> name.ToUpper(),
            InstanceNameFormatter = name => name.ToUpper(),
            BuilderFactory = () => new ResiliencePipelineBuilder
            {
                Name = "default-pipeline",
                InstanceName = "default-instance",
                TelemetryListener = new ConsoleTelemetryListener(),
            },
            PipelineComparer = StringComparer.OrdinalIgnoreCase,
        });

        // Get or create a cached pipeline
        var pipeline = registry.GetOrAddPipeline(
            "pipeline-1", 
            builder =>
            {
                builder.AddTimeout(TimeSpan.FromMilliseconds(10));
            });

        try
        {
            await pipeline.ExecuteAsync(async cancellationToken => await Task.Delay(100, cancellationToken), cancellationToken);
        }
        catch (TimeoutRejectedException)
        {
            // Ok, expected
        }
    }
}

public class ConsoleTelemetryListener : TelemetryListener
{
    public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
    {
        Console.WriteLine(
            "Pipeline: {0}, Instance: {1}, Strategy: {2}, Event: {3}", 
            args.Source.PipelineName, 
            args.Source.PipelineInstanceName, 
            args.Source.StrategyName ?? "(null)", 
            args.Event.EventName);
    }
}