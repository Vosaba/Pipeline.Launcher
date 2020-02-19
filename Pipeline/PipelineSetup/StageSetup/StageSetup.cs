using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;

namespace PipelineLauncher.StageSetup
{
    internal class StageSetup : IStageSetup
    {
        private IDataflowBlock _executionBlock;
        private Func<StageCreationContext, IDataflowBlock> CreateExecutionBlock { get; }

        public IDataflowBlock RetrieveExecutionBlock(StageCreationContext context)
        {
            return _executionBlock ??= CreateExecutionBlock(context);
        }

        public IList<IStageSetup> Next { get; set; } = new List<IStageSetup>();
        public IStageSetup Previous { get; set; }

        public StageSetup(Func<StageCreationContext, IDataflowBlock> createTerra)
        {
            CreateExecutionBlock = createTerra;
        }

        public void DestroyExecutionBlock()
        {
            _executionBlock = null;
        }
    }

    internal class StageSetup<TIn, TOut> : StageSetupOut<TOut>, IStageSetup<TIn, TOut>
    {
        public StageSetup(Func<StageCreationContext, IPropagatorBlock<PipelineStageItem<TIn>, PipelineStageItem<TOut>>> createTerra)
            : base(createTerra)
        { }

        public new IPropagatorBlock<PipelineStageItem<TIn>, PipelineStageItem<TOut>> RetrieveExecutionBlock(StageCreationContext context)
            => (IPropagatorBlock<PipelineStageItem<TIn>, PipelineStageItem<TOut>>)base.RetrieveExecutionBlock(context);

        ITargetBlock<PipelineStageItem<TIn>> IStageSetupIn<TIn>.RetrieveExecutionBlock(StageCreationContext context) => RetrieveExecutionBlock(context);

        //Func<StageCreationContext, IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>> IStage<TIn, TOut>.CreateExecutionBlock => (Func<StageCreationContext, IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>>)CreateExecutionBlock;

        //Func<StageCreationContext, ITargetBlock<PipelineItem<TIn>>> IStageIn<TIn>.CreateExecutionBlock => (Func<StageCreationContext, ITargetBlock<PipelineItem<TIn>>>)CreateExecutionBlock;

    }
}
