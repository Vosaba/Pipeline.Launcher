using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetupOut<TOut> : IStageSetup
    {
        new ISourceBlock<PipelineStageItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);
       // new Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>> CreateExecutionBlock { get; }

    }
}