using System.Collections.Generic;

namespace PipelineLauncher.Pipelines
{

    public interface IPipelineRunner<in TInput, out TOutput> : IPipelineRunnerBase<TInput, TOutput>
    {
        bool Post(TInput input);
        bool Post(IEnumerable<TInput> input);
    }
}
