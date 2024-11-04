using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Diagnostics;
using Weather.Console.Final.Utils;

var builder = WebApplication.CreateBuilder();

// Register the HTTP client
builder.Services.AddHttpClient("weather", c => c.BaseAddress = new Uri("https://localhost:7100"))
    .AddStandardResilienceHandler()
    .Configure(options => 
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

        options.Retry.MaxRetryAttempts = 5;
        options.Retry.Delay = TimeSpan.Zero;

        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.FailureRatio = 0.9;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);

        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
    });

builder.Services.AddHttpClient("weather-hedged", c => c.BaseAddress = new Uri("https://localhost:7100"))
    .AddStandardHedgingHandler(routes =>
    {
        routes.ConfigureOrderedGroups(options => Routes.ConfigureEndpoints(options));
    })
    .Configure(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

        options.Hedging.MaxHedgedAttempts = 5;
        options.Hedging.Delay = TimeSpan.Zero;
        options.Hedging.OnHedging = args =>
        {
            HttpRequestMessage? request = args.ActionContext.GetRequestMessage();
            return default;
        };

        options.Endpoint.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
        options.Endpoint.CircuitBreaker.MinimumThroughput = 5;
        options.Endpoint.CircuitBreaker.FailureRatio = 0.9;
        options.Endpoint.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);

        options.Endpoint.Timeout.Timeout = TimeSpan.FromSeconds(1);
    });

// Create the HTTP client
var httpClient = builder.Build().Services
    .GetRequiredService<IHttpClientFactory>()
    .CreateClient("weather");

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
