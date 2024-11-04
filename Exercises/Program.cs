using Exercises;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Registry;

var cancellationToken = CancellationToken.None;
var files = Enumerable.Range(0, 50).Select(v => $"file{v}.txt").ToArray();
var registry = new ResiliencePipelineRegistry<string>();
var loggerFatory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Error));

IServiceCollection services = new ServiceCollection();
Exercise7.ConfigureServices(services);
var exercise = services.BuildServiceProvider().GetRequiredService<Exercise7>();
await exercise.Run(files, cancellationToken);
