using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetupOut<TOut> : IStageSetup
    {
        new ISourceBlock<PipelineStageItem<TOut>> RetrieveExecutionBlock(StageCreationContext context);
       // new Func<StageCreationContext, ISourceBlock<PipelineItem<TOut>>> CreateExecutionBlock { get; }

    }
}