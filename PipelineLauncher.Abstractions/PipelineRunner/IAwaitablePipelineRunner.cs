using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.PipelineRunner
{
    public interface IAwaitablePipelineRunner<in TInput, TOutput> : IPipelineRunnerBase<TInput, TOutput>
    {
        IEnumerable<TOutput> Process(TInput input);
        IEnumerable<TOutput> Process(IEnumerable<TInput> input);

        Task<IEnumerable<TOutput>> ProcessAsync(TInput input);
        Task<IEnumerable<TOutput>> ProcessAsync(IEnumerable<TInput> input);

        IAsyncEnumerable<TOutput> ProcessAsyncEnumerable(TInput input);
        IAsyncEnumerable<TOutput> ProcessAsyncEnumerable(IEnumerable<TInput> input);

        Task GetCompletionTaskFor(TInput input);
        Task GetCompletionTaskFor(IEnumerable<TInput> input);

        IAwaitablePipelineRunner<TInput, TOutput> SetupExceptionHandler(Action<ExceptionItemsEventArgs> exceptionHandler);

        new IAwaitablePipelineRunner<TInput, TOutput> SetupCancellationToken(CancellationToken cancellationToken);
    }
}