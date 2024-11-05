using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace Extensions;

// This example demonstrates how to enable dynamic reloads of the pipeline based on the options.
internal class Advanced
{
    public static void Example(IServiceCollection services)
    {
        SetupOptions(services);

        services.AddResiliencePipeline("my-pipeline", (builder, context) =>
        {
            context.EnableReloads<RetryStrategyOptions>();

            var options = context.GetOptions<RetryStrategyOptions>();
            Console.WriteLine("Configuring retry strategy, retries: {0}", options.MaxRetryAttempts);

            builder.AddRetry(context.GetOptions<RetryStrategyOptions>());
        });

        // Materialize the pipeline
        var pipeline = services.BuildServiceProvider().GetRequiredKeyedService<ResiliencePipeline>("my-pipeline");

        Console.ReadLine();
    }

    private static void SetupOptions(IServiceCollection services)
    {
        // Options and configuraiton setup.
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, reloadOnChange: true)
            .Build();

        services.AddSingleton<IConfiguration>(config);
        services
            .AddOptions<RetryStrategyOptions>()
            .BindConfiguration("Retry");
    }
}
