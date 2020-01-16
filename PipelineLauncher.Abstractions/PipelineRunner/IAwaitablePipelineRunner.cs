using System;
using System.Collections.Generic;
using System.Threading;
using PipelineLauncher.Abstractions.PipelineEvents;

namespace PipelineLauncher.Abstractions.PipelineRunner
{
    public interface IAwaitablePipelineRunner<in TInput, out TOutput> : IPipelineRunnerBase<TInput, TOutput>
    {
        IEnumerable<TOutput> Process(TInput input);
        IEnumerable<TOutput> Process(IEnumerable<TInput> input);

        IAwaitablePipelineRunner<TInput, TOutput> SetupExceptionHandler(Action<ExceptionItemsEventArgs> exceptionHandler);

        new IAwaitablePipelineRunner<TInput, TOutput> SetupCancellationToken(CancellationToken cancellationToken);
    }
}