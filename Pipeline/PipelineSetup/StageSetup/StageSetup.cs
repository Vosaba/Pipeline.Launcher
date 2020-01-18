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
        private Func<StageCreationOptions, IDataflowBlock> CreateExecutionBlock { get; }

        public IDataflowBlock RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false)
        {
            if (_executionBlock == null )
            {
                _executionBlock = CreateExecutionBlock(options);
            }

            return _executionBlock;
        }

        public StageBaseConfiguration PipelineBaseConfiguration { get; set; }
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
        public StageSetup(Func<StageCreationOptions, IPropagatorBlock<PipelineStageItem<TIn>, PipelineStageItem<TOut>>> createTerra)
            : base(createTerra)
        { }

        public new IPropagatorBlock<PipelineStageItem<TIn>, PipelineStageItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false)
            => (IPropagatorBlock<PipelineStageItem<TIn>, PipelineStageItem<TOut>>)base.RetrieveExecutionBlock(options, forceCreation);

        ITargetBlock<PipelineStageItem<TIn>> IStageSetupIn<TIn>.RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false) => RetrieveExecutionBlock(options, forceCreation);

        //Func<StageCreationOptions, IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>> IStage<TIn, TOut>.CreateExecutionBlock => (Func<StageCreationOptions, IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>>)CreateExecutionBlock;

        //Func<StageCreationOptions, ITargetBlock<PipelineItem<TIn>>> IStageIn<TIn>.CreateExecutionBlock => (Func<StageCreationOptions, ITargetBlock<PipelineItem<TIn>>>)CreateExecutionBlock;

    }
}
