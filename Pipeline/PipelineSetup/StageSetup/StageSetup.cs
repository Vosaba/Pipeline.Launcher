using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.StageSetup
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

        public void DestroyExecutionBlock()
        {
            _executionBlock = null;
        }
    }

    internal class StageSetup<TInput, TOutput> : SourceStageSetup<TOutput>, IStageSetup<TInput, TOutput>
    {
        public StageSetup(Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>> executionBlockCreator)
            : base(executionBlockCreator)
        { }

        public new IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>> RetrieveExecutionBlock(StageCreationContext context)
            => (IPropagatorBlock<PipelineStageItem<TInput>, PipelineStageItem<TOutput>>)base.RetrieveExecutionBlock(context);

        ITargetBlock<PipelineStageItem<TInput>> ITargetStageSetup<TInput>.RetrieveExecutionBlock(StageCreationContext context) => RetrieveExecutionBlock(context);
    }
}
