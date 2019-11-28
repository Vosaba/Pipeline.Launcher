using System;
using PipelineLauncher.Abstractions.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.Stages
{
    internal class Stage : IStage
    {
        public Action TerraForm { get; }
        public bool IsTerraFormed { get ; set ; }

        public IDataflowBlock ExecutionBlock { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public IList<IStage> Next { get; set; } = new List<IStage>();
        public IStage Previous { get; set; }


        public Stage(IDataflowBlock executionBlock, Action terraForm, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            ExecutionBlock = executionBlock;
            TerraForm = terraForm;
        }
    }

    internal class Stage<TIn, TOut> : StageOut<TOut>, IStage<TIn, TOut>
    {
        public Stage(IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> executionBlock, Action terraForm, CancellationToken cancellationToken)
            : base(executionBlock, terraForm, cancellationToken)
        { }

        ITargetBlock<PipelineItem<TIn>> IStageIn<TIn>.ExecutionBlock => (ITargetBlock<PipelineItem<TIn>>)ExecutionBlock;

        IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> IStage<TIn, TOut>.ExecutionBlock => (IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>>)ExecutionBlock;
    }


    internal class StageIn<TIn> : Stage, IStageIn<TIn>
    {
        public StageIn(ITargetBlock<PipelineItem<TIn>> executionBlock, Action terraForm, CancellationToken cancellationToken)
            : base(executionBlock, terraForm, cancellationToken)
        { }

        IDataflowBlock IStage.ExecutionBlock => ExecutionBlock;

        ITargetBlock<PipelineItem<TIn>> IStageIn<TIn>.ExecutionBlock => (ITargetBlock<PipelineItem<TIn>>)ExecutionBlock;
    }

    internal class StageOut<TOut> : Stage, IStageOut<TOut>
    {
        public StageOut(ISourceBlock<PipelineItem<TOut>> executionBlock, Action terraForm, CancellationToken cancellationToken)
            : base(executionBlock, terraForm, cancellationToken)
        { }

        IDataflowBlock IStage.ExecutionBlock => ExecutionBlock;

        ISourceBlock<PipelineItem<TOut>> IStageOut<TOut>.ExecutionBlock => (ISourceBlock<PipelineItem<TOut>>)ExecutionBlock;
    }
}
