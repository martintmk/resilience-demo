using Exercises.Utils;
using Microsoft.Extensions.DependencyInjection;
using Polly.Hedging;
using Polly.Registry;
using Polly.Testing;
using Polly.Timeout;

namespace Exercises.Tests;

public class PipelineTests
{
    [Fact]
    public void TestComposition()
    {
        // Arrange
        var services = new ServiceCollection();
        Exercise_Final.ConfigureServices(services);
        var pipelineProvider = services
            .BuildServiceProvider()
            .GetRequiredService<ResiliencePipelineProvider<string>>();

        // Act
        var pipeline = pipelineProvider.GetPipeline<ProcessingStatus>("file-pipeline");

        // Assert
        var descriptor = pipeline.GetPipelineDescriptor();

        Assert.Equal(2, descriptor.Strategies.Count());

        var hedging = Assert.IsType<HedgingStrategyOptions<ProcessingStatus>>(descriptor.Strategies[0].Options);
        Assert.Equal(TimeSpan.FromMilliseconds(100), hedging.Delay);
        Assert.Equal(5, hedging.MaxHedgedAttempts);

        var timeout = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[1].Options);
        Assert.Equal(TimeSpan.FromMilliseconds(300), timeout.Timeout);
    }
}