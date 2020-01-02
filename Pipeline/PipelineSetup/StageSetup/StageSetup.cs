using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.StageSetup
{
    internal class StageSetup : IStageSetup
    {
        private IDataflowBlock _executionBlock;
        private Func<StageCreationOptions, IDataflowBlock> CreateExecutionBlock { get; }

        public IDataflowBlock RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false)
        {
            if (_executionBlock == null )
            {
                _executionBlock = CreateExecutionBlock(options);
            }

            return _executionBlock;
        }

        public PipelineBaseConfiguration PipelineBaseConfiguration { get; set; }
        public IList<IStageSetup> Next { get; set; } = new List<IStageSetup>();
        public IStageSetup Previous { get; set; }

        public StageSetup(Func<StageCreationOptions, IDataflowBlock> createTerra)
        {
            CreateExecutionBlock = createTerra;
        }

        public void DestroyBlock()
        {
            _executionBlock = null;
        }
    }

    internal class StageSetup<TIn, TOut> : StageSetupOut<TOut>, IStageSetup<TIn, TOut>
    {
        public StageSetup(Func<StageCreationOptions, IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>> createTerra)
            : base(createTerra)
        { }

        public new IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false)
            => (IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>)base.RetrieveExecutionBlock(options, forceCreation);

        ITargetBlock<PipelineItem<TIn>> IStageSetupIn<TIn>.RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false) => RetrieveExecutionBlock(options, forceCreation);

        //Func<StageCreationOptions, IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>> IStage<TIn, TOut>.CreateExecutionBlock => (Func<StageCreationOptions, IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>>)CreateExecutionBlock;

        //Func<StageCreationOptions, ITargetBlock<PipelineItem<TIn>>> IStageIn<TIn>.CreateExecutionBlock => (Func<StageCreationOptions, ITargetBlock<PipelineItem<TIn>>>)CreateExecutionBlock;

    }
}
