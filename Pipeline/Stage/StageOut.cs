using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stage
{
    internal class StageOut<TOut> : Stage, IStageOut<TOut>
    {
        public StageOut(Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>> createTerra)
            : base(createTerra)
        { }

        public new ISourceBlock<PipelineItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false)
            => (ISourceBlock<PipelineItem<TOut>>) base.RetrieveExecutionBlock(options, forceCreation);

        //public new Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>> CreateExecutionBlock => (Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>>)base.CreateExecutionBlock;
    }
}