
using Pipelines;
using Polly;

var pipeline = ResiliencePipeline.Empty;

await Execution_ResilienceContext.Example(pipeline, CancellationToken.None);