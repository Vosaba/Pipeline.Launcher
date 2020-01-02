using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;
using System.Threading.Tasks;

namespace PipelineLauncher.Abstractions.Pipeline
{
    public interface IPipelineStage<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        Task<PipelineItem<TOutput>> InternalExecute(PipelineItem<TInput> input, PipelineStageContext context);
        StageConfiguration Configuration { get; }
    }
}