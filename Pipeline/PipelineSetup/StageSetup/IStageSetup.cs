using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetup
    {
        IDataflowBlock RetrieveExecutionBlock(StageCreationContext context);

        void DestroyExecutionBlock();

        IList<IStageSetup> NextStageSetup { get; set; }

        IStageSetup PreviousStageSetup { get; set; }
    }

    public interface IStageSetup<TInput, TOutput> : ITargetStageSetup<TInput>, ISourceStageSetup<TOutput>
    {
        new IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> RetrieveExecutionBlock(StageCreationContext context);
    }
}