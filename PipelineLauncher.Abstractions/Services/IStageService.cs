using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher.Abstractions.Services
{
    public interface IStageService
    {
        TPipelineStage GetStageInstance<TPipelineStage>() where TPipelineStage : class, IPipeline;
    }
}