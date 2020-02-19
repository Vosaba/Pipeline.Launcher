using System;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    internal class StageSetupIn<TIn> : StageSetup, IStageSetupIn<TIn>
    {
        public StageSetupIn(Func<StageCreationContext, ITargetBlock<PipelineStageItem<TIn>>> createTerra)
            : base(createTerra)
        { }

        public new ITargetBlock<PipelineStageItem<TIn>> RetrieveExecutionBlock(StageCreationContext context)
            => (ITargetBlock<PipelineStageItem<TIn>>)base.RetrieveExecutionBlock(context);

        //public new Func<StageCreationContext, ITargetBlock<PipelineItem<TIn>>> CreateExecutionBlock =>
        //    (Func<StageCreationContext, ITargetBlock<PipelineItem<TIn>>>) base.CreateExecutionBlock;
    }
}