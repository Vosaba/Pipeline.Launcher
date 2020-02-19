using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetupIn<TIn> : IStageSetup
    {
        new ITargetBlock<PipelineStageItem<TIn>> RetrieveExecutionBlock(StageCreationContext context);
    }
}
