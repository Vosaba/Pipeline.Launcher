using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Extensions;
using PipelineLauncher.PipelineStage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineSetup.StageSetup
{
    internal class Stage : IStage
    {
        private IDataflowBlock _dataflowBlock;

        public IStage PreviousStage { get; set; }

        public IList<IStage> NextStageSetup { get; set; } = new List<IStage>();

        public BlockCreator BlockCreator { get; }

        public Stage(BlockCreator executionBlockCreator)
        {
            BlockCreator = executionBlockCreator;
        }

        public IDataflowBlock CreateBlock(StageCreationContext stageCreationContext)
        {
            return _dataflowBlock ??= BlockCreator(stageCreationContext);
        }

        public void DestroyBlock()
        {
            _dataflowBlock = null;
        }
    }

    //internal class SourceStageSetup<TOutput> : StageSetup, ISourceStageSetup<TOutput>
    //{
    //    private readonly SourceBlockCreator<TOutput> _sourceBlockCreator;

    //    public SourceStageSetup(SourceBlockCreator<TOutput> sourceBlockCreator)
    //        : base(x => sourceBlockCreator(x))
    //    {
    //        _sourceBlockCreator = sourceBlockCreator;
    //    }

    //    SourceBlockCreator<TOutput> ISourceStageSetup<TOutput>.BlockCreator => _sourceBlockCreator;
    //}

    //internal class TargetStageSetup<TInput> : StageSetup, ITargetStageSetup<TInput>
    //{
    //    private readonly TargetBlockCreator<TInput> _targetBlockCreator;
    //    private readonly TargetBlockPredicateCreator<TInput> _blockPredicateCreator;

    //    public TargetStageSetup(TargetBlockCreator<TInput> targetBlockCreator, TargetBlockPredicateCreator<TInput> blockPredicateCreator)
    //        : base(x => targetBlockCreator(x))
    //    {
    //        _targetBlockCreator = targetBlockCreator;
    //        _blockPredicateCreator = blockPredicateCreator;
    //    }

    //    public TargetBlockPredicateCreator<TInput> BlockPredicateCreator => throw new NotImplementedException();

    //    TargetBlockCreator<TInput> ITargetStageSetup<TInput>.BlockCreator => throw new NotImplementedException();
    //}

    internal class Stage<TInput, TOutput> : Stage, IStage<TInput, TOutput>
    {
        public new BlockCreator<TInput, TOutput> BlockCreator { get; }
        public TargetBlockPredicateCreator<TInput> BlockPredicateCreator { get; }
        public TargetBlockCreator<TInput> TargetBlockCreator => x => BlockCreator(x);
        public SourceBlockCreator<TOutput> SourceBlockCreator => x => BlockCreator(x);
        
        public Stage(BlockCreator<TInput, TOutput> blockCreator, TargetBlockPredicateCreator<TInput> blockPredicateCreator)
            : base(x => blockCreator(x))
        {
            BlockCreator = blockCreator;
            BlockPredicateCreator = blockPredicateCreator;
        }

        public void LinkTo(ITargetStage<TOutput> targetStage, StageCreationContext stageCreationContext)
        {
            var block = CreateBlock(stageCreationContext);
            var targetBlock = targetStage.CreateTargetBlock(stageCreationContext);
            var targetBlockPredicate = targetStage.BlockPredicateCreator(stageCreationContext);

            block.LinkTo(targetBlock, targetBlockPredicate.GetPredicate(targetBlock));
        }

        public ITargetBlock<PipelineStageItem<TInput>> CreateTargetBlock(StageCreationContext stageCreationContext)
        {
            throw new NotImplementedException();
        }

        public ISourceBlock<PipelineStageItem<TOutput>> CreateSourceBlock(StageCreationContext stageCreationContext)
        {
            throw new NotImplementedException();
        }

        public new IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> CreateBlock(StageCreationContext stageCreationContext)
        {
            throw new NotImplementedException();
        }
    }
}
