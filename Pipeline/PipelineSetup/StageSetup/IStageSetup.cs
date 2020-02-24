using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.PipelineStage;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineSetup.StageSetup
{
    internal interface IStageSetup
    {
        IDataflowBlock RetrieveExecutionBlock(StageCreationContext context);

        void DestroyExecutionBlock();

        IList<IStageSetup> NextStageSetup { get; set; }

        IStageSetup PreviousStageSetup { get; set; }

        IStageSetup CreateDeepCopy();
    }

    internal interface IStageSetup<TInput, TOutput> : ITargetStageSetup<TInput>, ISourceStageSetup<TOutput>
    {
        new IStageSetup<TInput, TOutput> CreateDeepCopy();
        new IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> RetrieveExecutionBlock(StageCreationContext context);
    }
}