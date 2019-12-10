using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stage
{
    internal class StageOut<TOut> : Stage, IStageOut<TOut>
    {
        public StageOut(Func<ISourceBlock<PipelineItem<TOut>>> createTerra, CancellationToken cancellationToken)
            : base(createTerra, cancellationToken)
        { }

        IDataflowBlock IStage.ExecutionBlock => ExecutionBlock;
        ISourceBlock<PipelineItem<TOut>> IStageOut<TOut>.ExecutionBlock => (ISourceBlock<PipelineItem<TOut>>)ExecutionBlock;
    }
}