using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Abstractions.Services
{
    public interface IStageService
    {
        TStage GetStageInstance<TStage>() where TStage : class, IStage;
    }
}