using System.Collections.Generic;
using System.Threading;

namespace PipelineLauncher.Abstractions.PipelineRunner
{
    public interface IPipelineRunner<in TInput, out TOutput> : IPipelineRunnerBase<TInput, TOutput>
    {
        bool Post(TInput input);
        bool Post(IEnumerable<TInput> input);

        new IPipelineRunner<TInput, TOutput> SetupCancellationToken(CancellationToken cancellationToken);
    }
}
