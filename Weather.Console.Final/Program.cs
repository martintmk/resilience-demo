using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Diagnostics;

var app = WebApplication.CreateBuilder();
var services = app.Services;

services
    .AddHttpClient("my-client", client => client.BaseAddress = new Uri("https://localhost:7100"))
    .AddHttpMessageHandler(() =>
    {
        var options = new HttpStandardResilienceOptions();

        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
        options.CircuitBreaker.FailureRatio = 0.9;
        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);

        options.Retry.MaxRetryAttempts = 5;
        options.Retry.Delay = TimeSpan.Zero;
        options.AttemptTimeout.Timeout = TimeSpan.FromMilliseconds(100);

        var resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddTimeout(options.TotalRequestTimeout)
            .AddRateLimiter(options.RateLimiter)
            .AddRetry(options.Retry)
            .AddCircuitBreaker(options.CircuitBreaker)
            .AddTimeout(options.AttemptTimeout)
            .Build();

        return new ResilienceHandler(resiliencePipeline);
    });

var httpClient = app.Build().Services.GetRequiredService<IHttpClientFactory>().CreateClient("my-client");

while (true)
{
    await Batch(async () =>
    {
        var watch = Stopwatch.StartNew();

        try
        {
            using var response = await httpClient.GetAsync("weatherforecast");

            Console.WriteLine($"{(int)response.StatusCode}: {watch.Elapsed.TotalMilliseconds,10:0.00}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Err: {watch.Elapsed.TotalMilliseconds,10:0.00}ms ({ex.GetType().Name})");
        }
    });

    Console.ReadLine();
}


async Task Batch(Func<Task> action, int count = 10)
{
    var watch = Stopwatch.StartNew();
    Console.WriteLine("----------------------------------------------------------");
    Console.WriteLine($"Sending {count} requests...");
    Console.WriteLine();

    for (int i = 0; i < count; i++)
    {
        await action();
    }

    Console.WriteLine();
    Console.WriteLine($"Sending {count} requests...{watch.Elapsed.TotalMilliseconds}ms");
    Console.WriteLine();
}

