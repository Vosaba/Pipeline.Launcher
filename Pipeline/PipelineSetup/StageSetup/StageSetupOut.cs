using System;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    internal class StageSetupOut<TOut> : StageSetup, IStageSetupOut<TOut>
    {
        public StageSetupOut(Func<StageCreationContext, ISourceBlock<PipelineStageItem<TOut>>> createTerra)
            : base(createTerra)
        { }

        public new ISourceBlock<PipelineStageItem<TOut>> RetrieveExecutionBlock(StageCreationContext context)
            => (ISourceBlock<PipelineStageItem<TOut>>) base.RetrieveExecutionBlock(context);

        //public new Func<StageCreationContext, ISourceBlock<PipelineItem<TOut>>> CreateExecutionBlock => (Func<StageCreationContext, ISourceBlock<PipelineItem<TOut>>>)base.CreateExecutionBlock;
    }
}