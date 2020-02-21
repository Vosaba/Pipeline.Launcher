using System;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    internal class SourceStageSetup<TOutput> : StageSetup, ISourceStageSetup<TOutput>
    {
        public SourceStageSetup(Func<StageCreationContext, ISourceBlock<PipelineStageItem<TOutput>>> executionBlockCreator)
            : base(executionBlockCreator)
        { }

        public new ISourceBlock<PipelineStageItem<TOutput>> RetrieveExecutionBlock(StageCreationContext context)
            => (ISourceBlock<PipelineStageItem<TOutput>>)base.RetrieveExecutionBlock(context);
    }
}