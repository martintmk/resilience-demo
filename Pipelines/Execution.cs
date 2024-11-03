using Polly;

namespace Pipelines;

// Demonstration of how pipelines can be executed.
internal static class Execution
{
    public static async Task Example(ResiliencePipeline pipeline, CancellationToken cancellationToken)
    {
        // Void callback
        await pipeline.ExecuteAsync(
            async cancellationToken =>
            {
                await Task.Delay(1000, cancellationToken);
                Console.WriteLine("Hello World!");
            },
            cancellationToken);


        // String-based callback
        string string_result = await pipeline.ExecuteAsync(
            async cancellationToken =>
            {
                await Task.Delay(1000, cancellationToken);
                return "Hello World!";
            },
            cancellationToken);

        // Int-based callback
        int int_result = await pipeline.ExecuteAsync(
            async cancellationToken =>
            {
                await Task.Delay(1000, cancellationToken);
                return 100;
            },
            cancellationToken);

        // Using state
        await pipeline.ExecuteAsync(
            static async (state, cancellationToken) =>
            {
                Console.WriteLine(state);
            },
            "Hello World!",
            cancellationToken);

        // Using state with multiple parameters
        await pipeline.ExecuteAsync(
            static async (state, cancellationToken) =>
            {
                Console.WriteLine("{0} at {1}", state.Item1, state.Item2);
            },
            ("Hello World!", DateTime.UtcNow),
            cancellationToken);
    }
}
