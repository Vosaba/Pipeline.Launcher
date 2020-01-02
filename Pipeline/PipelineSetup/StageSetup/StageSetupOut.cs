using System;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;

namespace PipelineLauncher.StageSetup
{
    internal class StageSetupOut<TOut> : StageSetup, IStageSetupOut<TOut>
    {
        public StageSetupOut(Func<StageCreationOptions, ISourceBlock<PipelineStageItem<TOut>>> createTerra)
            : base(createTerra)
        { }

        public new ISourceBlock<PipelineStageItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false)
            => (ISourceBlock<PipelineStageItem<TOut>>) base.RetrieveExecutionBlock(options, forceCreation);

        //public new Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>> CreateExecutionBlock => (Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>>)base.CreateExecutionBlock;
    }
}