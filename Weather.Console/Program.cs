using System.Diagnostics;

var builder = WebApplication.CreateBuilder();

// Register the HTTP client
builder.Services.AddHttpClient("weather", c => c.BaseAddress = new Uri("https://localhost:7100"));

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
