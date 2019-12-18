using System.Collections.Generic;

namespace PipelineLauncher.Pipelines
{
    public interface IAwaitablePipelineRunner<in TInput, out TOutput> : IPipelineRunner<TInput, TOutput>
    {
        IEnumerable<TOutput> Process(TInput input);
        IEnumerable<TOutput> Process(IEnumerable<TInput> input);

        IAsyncEnumerable<TOutput> Process(IEnumerable<TInput> input, bool f);
    }
}