using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.Pipeline;
//using PipelineLauncher.Dataflow;

namespace PipelineLauncher.Stages
{
    internal class Stage: IStage
    {
        public IDataflowBlock ExecutionBlock { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public IStage Next { get; set; }
        
        public IStage Previous { get; set; }

        public Stage(IDataflowBlock executionBlock, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            ExecutionBlock = executionBlock;
        }
    }

    internal class Stage <TIn, TOut>: StageOut<TOut>, IStage<TIn, TOut>
    {
        //public new IPropagatorBlock<TIn, TOut> ExecutionBlock { get; }

        public Stage(IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> executionBlock, CancellationToken cancellationToken) : base(executionBlock, cancellationToken)
        {
            //ExecutionBlock = executionBlock;
        }

        ITargetBlock<PipelineItem<TIn>> IStageIn<TIn>.ExecutionBlock => (ITargetBlock< PipelineItem<TIn>>) ExecutionBlock;

        IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> IStage<TIn, TOut>.ExecutionBlock => (IPropagatorBlock< PipelineItem<TIn>, PipelineItem<TOut>>) ExecutionBlock;
    }


    internal class StageIn<TIn> : Stage, IStageIn<TIn>
    {
        public StageIn(ITargetBlock<PipelineItem<TIn>> executionBlock, CancellationToken cancellationToken): base(executionBlock, cancellationToken)
        {
            //ExecutionBlock = executionBlock;
        }

        //public ITargetBlock<TIn> ExecutionBlock { get; }

        IDataflowBlock IStage.ExecutionBlock => ExecutionBlock;
        ITargetBlock<PipelineItem<TIn>> IStageIn<TIn>.ExecutionBlock => (ITargetBlock< PipelineItem<TIn>>) ExecutionBlock;

        //IStage IStage.Previous
        //{
        //    get => Previous;
        //    set => Previous = (IStageOut<TIn>)value;
        //}

    }

    internal class StageOut<TOut> : Stage, IStageOut<TOut>
    {
        public StageOut(ISourceBlock<PipelineItem<TOut>> executionBlock, CancellationToken cancellationToken) : base(executionBlock, cancellationToken)
        {
           // ExecutionBlock = executionBlock;
        }

        //public ISourceBlock<TOut> ExecutionBlock { get; }

        IDataflowBlock IStage.ExecutionBlock => ExecutionBlock;
        ISourceBlock<PipelineItem<TOut>> IStageOut<TOut>.ExecutionBlock => (ISourceBlock< PipelineItem<TOut>>) ExecutionBlock;
    }
}
