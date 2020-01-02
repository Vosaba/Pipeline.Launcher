using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Abstractions.Services
{
    public interface IStageService
    {
        TPipelineStage GetStageInstance<TPipelineStage>() where TPipelineStage : class, IStage;
    }
}