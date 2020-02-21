using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineSetup.StageSetup
{
    public interface ITargetStageSetup<TInput> : IStageSetup
    {
        new ITargetBlock<PipelineStageItem<TInput>> RetrieveExecutionBlock(StageCreationContext context);
    }
}
