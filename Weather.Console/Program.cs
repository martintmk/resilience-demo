using System.Diagnostics;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
IServiceCollection services = builder.Services;

services
    .AddHttpClient("weather", client => client.BaseAddress = new Uri("https://localhost:7100"));

// Use the client
var host = builder.Build();
var httpClient = host.Services
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
            Console.WriteLine($"{(int)response.StatusCode}: {watch.Elapsed.TotalMilliseconds}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.GetType().Name}: {watch.Elapsed.TotalMilliseconds}ms");
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