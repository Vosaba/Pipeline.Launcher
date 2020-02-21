using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.Abstractions.Services
{
    public interface IStageService
    {
        TStage GetStageInstance<TStage>() where TStage : class, IStage;
    }
}