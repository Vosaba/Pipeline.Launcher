using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    public interface ISourceStageSetup<TOutput> : IStageSetup
    {
        new ISourceBlock<PipelineStageItem<TOutput>> RetrieveExecutionBlock(StageCreationContext context);
    }
}