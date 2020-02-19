using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Abstractions.PipelineStage
{
    public interface IPipelineBulkStage<TInput, TOutput> : IStage<TInput, TOutput>
    {
        Task<IEnumerable<TOutput>> ExecuteAsync(TInput[] input, CancellationToken cancellationToken);
        BulkStageConfiguration Configuration { get; }
    }
}