using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.PipelineStage;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineSetup.StageSetup
{
    internal delegate IDataflowBlock BlockCreator(StageCreationContext stageCreationContext);
    internal delegate ITargetBlock<PipelineStageItem<TInput>> TargetBlockCreator<TInput>(StageCreationContext stageCreationContext);
    internal delegate ISourceBlock<PipelineStageItem<TOutput>> SourceBlockCreator<TOutput>(StageCreationContext stageCreationContext);
    internal delegate IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> BlockCreator<TInput, TOutput>(StageCreationContext stageCreationContext);

    internal delegate IPipelinePredicate<PipelineStageItem<TInput>> TargetBlockPredicateCreator<TInput>(StageCreationContext stageCreationContext);
    //internal delegate IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> PropagatorBlockPredicateCreator<TInput, TOutput>(StageCreationContext stageCreationContext);

    internal interface IStageSetup
    {
        BlockCreator BlockCreator { get; }

        IList<IStageSetup> NextStageSetup { get; set; }

        IStageSetup PreviousStageSetup { get; set; }
    }

    internal interface ISourceStageSetup<TOutput> : IStageSetup
    {
        new SourceBlockCreator<TOutput> BlockCreator { get; }
    }

    internal interface ITargetStageSetup<TInput> : IStageSetup
    {
        new TargetBlockCreator<TInput> BlockCreator { get; }

        TargetBlockPredicateCreator<TInput> BlockPredicateCreator { get; }
    }

    internal interface IStageSetup<TInput, TOutput> : ITargetStageSetup<TInput>, ISourceStageSetup<TOutput>
    {
        new BlockCreator<TInput, TOutput> BlockCreator { get; }
    }
}