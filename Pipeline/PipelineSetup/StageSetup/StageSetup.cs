using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.PipelineStage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineSetup.StageSetup
{
    internal class StageSetup : IStageSetup
    {
        public IStageSetup PreviousStageSetup { get; set; }

        public IList<IStageSetup> NextStageSetup { get; set; } = new List<IStageSetup>();

        public BlockCreator BlockCreator { get; }

        public StageSetup(BlockCreator executionBlockCreator)
        {
            BlockCreator = executionBlockCreator;
        }
    }

    internal class SourceStageSetup<TOutput> : StageSetup, ISourceStageSetup<TOutput>
    {
        private readonly SourceBlockCreator<TOutput> _sourceBlockCreator;

        public SourceStageSetup(SourceBlockCreator<TOutput> sourceBlockCreator)
            : base(x => sourceBlockCreator(x))
        {
            _sourceBlockCreator = sourceBlockCreator;
        }

        SourceBlockCreator<TOutput> ISourceStageSetup<TOutput>.BlockCreator => _sourceBlockCreator;
    }

    internal class TargetStageSetup<TInput> : StageSetup, ITargetStageSetup<TInput>
    {
        private readonly TargetBlockCreator<TInput> _targetBlockCreator;
        private readonly TargetBlockPredicateCreator<TInput> _blockPredicateCreator;

        public TargetStageSetup(TargetBlockCreator<TInput> targetBlockCreator, TargetBlockPredicateCreator<TInput> blockPredicateCreator)
            : base(x => targetBlockCreator(x))
        {
            _executionBlockCreator = executionBlockCreator;
            _executionBlockCreatorPredicate = executionBlockCreatorPredicate;
        }

        public TargetBlockPredicateCreator<TInput> BlockPredicateCreator => throw new NotImplementedException();

        TargetBlockCreator<TInput> ITargetStageSetup<TInput>.BlockCreator => throw new NotImplementedException();
    }

    internal class StageSetup<TInput, TOutput> : SourceStageSetup<TOutput>, IStageSetup<TInput, TOutput>
    {
        private readonly Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>> _executionBlockCreator;
        private readonly Func<StageCreationContext, IPipelinePredicate<PipelineStageItem<TInput>>> _executionBlockCreatorPredicate;

        public StageSetup(
            Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>>executionBlockCreator, 
            Func<StageCreationContext, IPipelinePredicate<PipelineStageItem<TInput>>> executionBlockCreatorPredicate)
            : base(executionBlockCreator, executionBlockCreatorPredicate)
        {
            _executionBlockCreator = executionBlockCreator;
            _executionBlockCreatorPredicate = executionBlockCreatorPredicate;
        }

        //ITargetStageSetup<TInput> ITargetStageSetup<TInput>.CreateDeepCopy() => CreateDeepCopy();

        //public new IStageSetup<TInput, TOutput> CreateDeepCopy()
        //    => new StageSetup<TInput, TOutput>(_executionBlockCreator, null);

        public new IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> RetrieveExecutionBlock(StageCreationContext context)
            => (IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>)base.RetrieveExecutionBlock(context);

        public new IPipelinePredicate<PipelineStageItem<TInput>> RetrieveExecutionBlockPredicate(
            StageCreationContext context)
            => _executionBlockCreatorPredicate(context);

        ITargetBlock<PipelineStageItem<TInput>> ITargetStageSetup<TInput>.RetrieveExecutionBlock(StageCreationContext context) => RetrieveExecutionBlock(context);
    }
}
