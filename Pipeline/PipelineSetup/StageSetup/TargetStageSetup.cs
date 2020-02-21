using System;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    internal class TargetStageSetup<TIn> : StageSetup, ITargetStageSetup<TIn>
    {
        public TargetStageSetup(Func<StageCreationContext, ITargetBlock<PipelineStageItem<TIn>>> executionBlockCreator)
            : base(executionBlockCreator)
        { }

        public new ITargetBlock<PipelineStageItem<TIn>> RetrieveExecutionBlock(StageCreationContext context)
            => (ITargetBlock<PipelineStageItem<TIn>>)base.RetrieveExecutionBlock(context);

        //public new Func<StageCreationContext, ITargetBlock<PipelineItem<TIn>>> CreateExecutionExecutionBlock =>
        //    (Func<StageCreationContext, ITargetBlock<PipelineItem<TIn>>>) base.CreateExecutionExecutionBlock;
    }
}