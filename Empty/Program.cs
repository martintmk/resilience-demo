
using Polly;

ResiliencePipeline pipeline = ResiliencePipeline.Empty;

ResiliencePipeline<string> pipeline_generic = ResiliencePipeline<string>.Empty;