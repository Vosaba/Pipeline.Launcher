using System.Collections.Generic;

namespace PipelineLauncher.Abstractions.PipelineRunner
{
    public interface IAwaitablePipelineRunner<in TInput, out TOutput> : IPipelineRunnerBase<TInput, TOutput>
    {
        IEnumerable<TOutput> Process(TInput input);
        IEnumerable<TOutput> Process(IEnumerable<TInput> input);
    }
}