using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.Stages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineLauncher.Abstractions.PipelineStage
{
    public interface IBulkStage<TInput, TOutput> : IPipelineStage<TInput, TOutput>
    {
        Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input, CancellationToken cancellationToken);
        BulkStageConfiguration Configuration { get; }
    }
}