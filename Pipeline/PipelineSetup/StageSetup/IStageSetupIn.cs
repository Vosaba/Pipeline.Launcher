using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetupIn<TIn> : IStageSetup
    {
        new ITargetBlock<PipelineItem<TIn>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);
    }
}
