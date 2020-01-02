using System;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    internal class StageSetupIn<TIn> : StageSetup, IStageSetupIn<TIn>
    {
        public StageSetupIn(Func<StageCreationOptions, ITargetBlock<PipelineStageItem<TIn>>> createTerra)
            : base(createTerra)
        { }

        public new ITargetBlock<PipelineStageItem<TIn>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false)
            => (ITargetBlock<PipelineStageItem<TIn>>)base.RetrieveExecutionBlock(options, forceCreation);

        //public new Func<StageCreationOptions, ITargetBlock<PipelineItem<TIn>>> CreateExecutionBlock =>
        //    (Func<StageCreationOptions, ITargetBlock<PipelineItem<TIn>>>) base.CreateExecutionBlock;
    }
}