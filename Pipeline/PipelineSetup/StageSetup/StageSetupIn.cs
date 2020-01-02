using System;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.StageSetup
{
    internal class StageSetupIn<TIn> : StageSetup, IStageSetupIn<TIn>
    {
        public StageSetupIn(Func<StageCreationOptions, ITargetBlock<PipelineItem<TIn>>> createTerra)
            : base(createTerra)
        { }

        public new ITargetBlock<PipelineItem<TIn>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false)
            => (ITargetBlock<PipelineItem<TIn>>)base.RetrieveExecutionBlock(options, forceCreation);

        //public new Func<StageCreationOptions, ITargetBlock<PipelineItem<TIn>>> CreateExecutionBlock =>
        //    (Func<StageCreationOptions, ITargetBlock<PipelineItem<TIn>>>) base.CreateExecutionBlock;
    }
}