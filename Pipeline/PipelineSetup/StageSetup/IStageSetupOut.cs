using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetupOut<TOut> : IStageSetup
    {
        new ISourceBlock<PipelineItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);
       // new Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>> CreateExecutionBlock { get; }

    }
}