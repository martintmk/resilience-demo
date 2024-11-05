using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Telemetry;

// This example demonstrates how to add custom telemetry listeners and enrichers.
static class Telemetry
{
    public static async Task Example(IServiceCollection services, CancellationToken cancellationToken)
    {
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        services.Configure<TelemetryOptions>(options =>
        {
            options.TelemetryListeners.Add(new CustomTelemetryListener());
            options.MeteringEnrichers.Add(new CustomMeteringEnricher());
        });

        services.AddResiliencePipeline("my-pipeline", (builder, context) =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(10));

            // Create a new telemetry options from the global options.
            var options = new TelemetryOptions(context.GetOptions<TelemetryOptions>());

            // Configure the telemetry for this pipeline
            builder.ConfigureTelemetry(options);
        });

        // Retrieve the pipeline
        var pipeline = services.BuildServiceProvider().GetRequiredKeyedService<ResiliencePipeline>("my-pipeline");

        await pipeline.ExecuteAsync(
            async token => await Task.Delay(TimeSpan.FromSeconds(1)),
            cancellationToken);
    }


    public class CustomTelemetryListener : TelemetryListener
    {
        public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
        {
            if (args.Arguments is OnRetryArguments<TResult> retryArgs)
            {
                Console.WriteLine("Retry occurred, Attempt: {0}", retryArgs.AttemptNumber);
            }

            Console.WriteLine("Event Occured: {0}", args.Event.EventName);
        }
    }


    public class CustomMeteringEnricher : MeteringEnricher
    {
        public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
        {
            if (context.TelemetryEvent.Arguments is OnRetryArguments<TResult> retryArgs)
            {
                context.Tags.Add(new ("retry.attempt", retryArgs.AttemptNumber));
            }
        }
    }
}
