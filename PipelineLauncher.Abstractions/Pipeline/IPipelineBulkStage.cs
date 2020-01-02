using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineBulkStage<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        Task<IEnumerable<PipelineItem<TOutput>>> InternalExecute(IEnumerable<PipelineItem<TInput>> input, PipelineStageContext context);
        BulkStageConfiguration Configuration { get; }
    }
}