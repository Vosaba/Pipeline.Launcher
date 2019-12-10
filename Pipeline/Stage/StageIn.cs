using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stage
{
    internal class StageIn<TIn> : Stage, IStageIn<TIn>
    {
        public StageIn(Func<ITargetBlock<PipelineItem<TIn>>> createTerra, CancellationToken cancellationToken)
            : base(createTerra, cancellationToken)
        { }

        IDataflowBlock IStage.ExecutionBlock => ExecutionBlock;
        ITargetBlock<PipelineItem<TIn>> IStageIn<TIn>.ExecutionBlock => (ITargetBlock<PipelineItem<TIn>>)ExecutionBlock;
    }
}