using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.PipelineStage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineSetup.StageSetup
{
    internal class StageSetup : IStageSetup
    {
        private readonly Func<StageCreationContext, IDataflowBlock> _createExecutionBlock;
        private IDataflowBlock _executionBlock;

        public IStageSetup PreviousStageSetup { get; set; }
        public IList<IStageSetup> NextStageSetup { get; set; } = new List<IStageSetup>();

        public StageSetup(Func<StageCreationContext, IDataflowBlock> executionBlockCreator)
        {
            _createExecutionBlock = executionBlockCreator;
        }

        public IDataflowBlock RetrieveExecutionBlock(StageCreationContext context)
        {
            return _executionBlock ??= _createExecutionBlock(context);
        }

        public IStageSetup CreateDeepCopy()
        {
            return new StageSetup(_createExecutionBlock);
        }

        public void DestroyExecutionBlock()
        {
            _executionBlock = null;
        }
    }

    internal class StageSetup<TInput, TOutput> : SourceStageSetup<TOutput>, IStageSetup<TInput, TOutput>
    {
        private readonly Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>> _executionBlockCreator;

        public StageSetup(
            Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>>
                executionBlockCreator)
            : base(executionBlockCreator)
        {
            _executionBlockCreator = executionBlockCreator;
        }


        ITargetStageSetup<TInput> ITargetStageSetup<TInput>.CreateDeepCopy() => CreateDeepCopy();

        public new IStageSetup<TInput, TOutput> CreateDeepCopy()
            => new StageSetup<TInput, TOutput>(_executionBlockCreator);

        public new IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> RetrieveExecutionBlock(StageCreationContext context)
            => (IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>)base.RetrieveExecutionBlock(context);

        ITargetBlock<PipelineStageItem<TInput>> ITargetStageSetup<TInput>.RetrieveExecutionBlock(StageCreationContext context) => RetrieveExecutionBlock(context);
    }
}
