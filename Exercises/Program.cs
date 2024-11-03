using Exercises;
using Microsoft.Extensions.DependencyInjection;

var files = Enumerable.Range(0, 50).Select(v => $"file{v}.txt").ToArray();

var services = new ServiceCollection();

Exercise8.ConfigureServices(services);

await services.BuildServiceProvider().GetRequiredService<Exercise8>().Run(files, CancellationToken.None);





