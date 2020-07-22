using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineLauncher.Abstractions.PipelineRunner
{
    public interface IPipelineRunner<in TInput, out TOutput> : IPipelineRunnerBase<TInput, TOutput>
    {
        bool Post(TInput input);
        bool Post(IEnumerable<TInput> input);

        Task<bool> PostAsync(TInput input);
        Task<bool> PostAsync(IEnumerable<TInput> input);

        Task<bool> PostAsync(TInput input, CancellationToken cancellationToken);
        Task<bool> PostAsync(IEnumerable<TInput> input, CancellationToken cancellationToken);

        Task CompleteExecution(); 

        new IPipelineRunner<TInput, TOutput> SetupCancellationToken(CancellationToken cancellationToken);
    }
}
