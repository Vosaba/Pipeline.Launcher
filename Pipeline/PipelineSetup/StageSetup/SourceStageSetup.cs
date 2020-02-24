using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.PipelineStage;
using System;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.PipelineSetup.StageSetup
{
    internal class SourceStageSetup<TOutput> : StageSetup, ISourceStageSetup<TOutput>
    {
        private readonly Func<StageCreationContext, ISourceBlock<PipelineStageItem<TOutput>>> _executionBlockCreator;

        public SourceStageSetup(
            Func<StageCreationContext, ISourceBlock<PipelineStageItem<TOutput>>> executionBlockCreator)
            : base(executionBlockCreator)
        {
            _executionBlockCreator = executionBlockCreator;
        }

        public new ISourceStageSetup<TOutput> CreateDeepCopy() => new SourceStageSetup<TOutput>(_executionBlockCreator);

        public new ISourceBlock<PipelineStageItem<TOutput>> RetrieveExecutionBlock(StageCreationContext context)
            => (ISourceBlock<PipelineStageItem<TOutput>>)base.RetrieveExecutionBlock(context);
    }
}