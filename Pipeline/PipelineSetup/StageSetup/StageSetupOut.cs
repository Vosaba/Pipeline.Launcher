using System;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.StageSetup
{
    internal class StageSetupOut<TOut> : StageSetup, IStageSetupOut<TOut>
    {
        public StageSetupOut(Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>> createTerra)
            : base(createTerra)
        { }

        public new ISourceBlock<PipelineItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false)
            => (ISourceBlock<PipelineItem<TOut>>) base.RetrieveExecutionBlock(options, forceCreation);

        //public new Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>> CreateExecutionBlock => (Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>>)base.CreateExecutionBlock;
    }
}