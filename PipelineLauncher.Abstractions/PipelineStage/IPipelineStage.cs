using System.Threading.Tasks;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Abstractions.PipelineStage
{
    public interface IPipelineStage<TInput, TOutput> : IStage<TInput, TOutput>
    {
        Task<PipelineStageItem<TOutput>> InternalExecute(PipelineStageItem<TInput> input, PipelineStageContext context);
        StageConfiguration Configuration { get; }
    }
}