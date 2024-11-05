using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Registry;

namespace Extensions;

// This example demonstrates how to register and retrieve resilience pipelines using DI.
internal class Basics
{
    public static void Example(IServiceCollection services)
    {
        services.AddResiliencePipeline("my-pipeline", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(1));
        });

        services.AddResiliencePipeline<string, HttpResponseMessage>("my-pipeline", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(1));
        });

        services.TryAddSingleton<Consumer1>();
        services.TryAddSingleton<Consumer2>();

        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<Consumer1>();
        serviceProvider.GetRequiredService<Consumer2>();

        serviceProvider.GetRequiredKeyedService<ResiliencePipeline>("my-pipeline");
        serviceProvider.GetRequiredKeyedService<ResiliencePipeline<HttpResponseMessage>>("my-pipeline");
    }

    public class Consumer1
    {
        private readonly ResiliencePipeline _pipeline1;
        private readonly ResiliencePipeline<HttpResponseMessage> _pipeline2;

        public Consumer1(ResiliencePipelineProvider<string> provider)
        {
            _pipeline1 = provider.GetPipeline("my-pipeline");
            _pipeline2 = provider.GetPipeline<HttpResponseMessage>("my-pipeline");
        }
    }

    public class Consumer2
    {
        private readonly ResiliencePipeline _pipeline1;
        private readonly ResiliencePipeline<HttpResponseMessage> _pipeline2;

        public Consumer2(
            [FromKeyedServices("my-pipeline")]
            ResiliencePipeline pipeline1,

            [FromKeyedServices("my-pipeline")]
            ResiliencePipeline<HttpResponseMessage> pipeline2)
        {
            _pipeline1 = pipeline1;
            _pipeline2 = pipeline2;
        }
    }
}
