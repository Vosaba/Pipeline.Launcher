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

    internal delegate PipelinePredicate<TInput> TargetBlockPredicateCreator<TInput>(StageCreationContext stageCreationContext);
    //internal delegate IPropagatorBlock<PipelineStageItem<TInput>, PiapelineStageItem<TOutput>> PropagatorBlockPredicateCreator<TInput, TOutput>(StageCreationContext stageCreationContext);

    internal interface IStage
    {
        BlockCreator BlockCreator { get; }

        IDataflowBlock CreateBlock(StageCreationContext stageCreationContext);

        void DestroyBlock();

        IList<IStage> NextStageSetup { get; set; }

        IStage PreviousStage { get; set; }
    }

    internal interface ISourceStage<TOutput> : IStage
    {
        SourceBlockCreator<TOutput> SourceBlockCreator { get; }
        ISourceBlock<PipelineStageItem<TOutput>> CreateSourceBlock(StageCreationContext stageCreationContext);
    }

    internal interface ITargetStage<TInput> : IStage
    {
        TargetBlockCreator<TInput> TargetBlockCreator { get; }
        TargetBlockPredicateCreator<TInput> BlockPredicateCreator { get; }
        ITargetBlock<PipelineStageItem<TInput>> CreateTargetBlock(StageCreationContext stageCreationContext);
    }

    internal interface IStage<TInput, TOutput> : ITargetStage<TInput>, ISourceStage<TOutput>
    {
        new BlockCreator<TInput, TOutput> BlockCreator { get; }
        new IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> CreateBlock(StageCreationContext stageCreationContext);

        void LinkTo(ITargetStage<TOutput> targetStage, StageCreationContext stageCreationContext);
    }
}