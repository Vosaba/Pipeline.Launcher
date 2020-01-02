using System.Collections.Generic;

namespace PipelineLauncher.PipelineRunner
{
    public interface IAwaitablePipelineRunner<in TInput, out TOutput> : IPipelineRunnerBase<TInput, TOutput>
    {
        IEnumerable<TOutput> Process(TInput input);
        IEnumerable<TOutput> Process(IEnumerable<TInput> input);

        //IObservable<TOutput> ProcessAsObservable(TInput input);
        //IObservable<TOutput> ProcessAsObservable(IEnumerable<TInput> input);

        //IAsyncEnumerable<TOutput> Process(IEnumerable<TInput> input, bool f);
    }
}