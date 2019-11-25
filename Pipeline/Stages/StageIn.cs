using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Pipeline;
//using PipelineLauncher.Dataflow;

namespace PipelineLauncher.Stages
{
    internal class Stage <TIn, TOut>: StageOut<TOut>, IStage<TIn, TOut>
    {
        public new IPropagatorBlock<TIn, TOut> ExecutionBlock { get; }

        public Stage(IPropagatorBlock<TIn, TOut> executionBlock) : base(executionBlock)
        {
            ExecutionBlock = executionBlock;
        }

        ITargetBlock<TIn> IStageIn<TIn>.ExecutionBlock => ExecutionBlock;

        IPropagatorBlock<TIn, TOut> IStage<TIn, TOut>.ExecutionBlock => ExecutionBlock;
    }


    internal class StageIn<TIn> : IStageIn<TIn>
    {
        public StageIn(ITargetBlock<TIn> executionBlock)
        {
            ExecutionBlock = executionBlock;
        }

        public ITargetBlock<TIn> ExecutionBlock { get; }

        public IStage Previous { get; set; }

        public IStage Next { get; set; }

        IDataflowBlock IStage.ExecutionBlock => ExecutionBlock;
        ITargetBlock<TIn> IStageIn<TIn>.ExecutionBlock => ExecutionBlock;

        //IStage IStage.Previous
        //{
        //    get => Previous;
        //    set => Previous = (IStageOut<TIn>)value;
        //}

    }

    internal class StageOut<TOut> : IStageOut<TOut>
    {
        public StageOut(ISourceBlock<TOut> executionBlock)
        {
            ExecutionBlock = executionBlock;
        }

        public ISourceBlock<TOut> ExecutionBlock { get; }
        public IStage Next { get; set; }
        public IStage Previous { get; set; }

        IDataflowBlock IStage.ExecutionBlock => ExecutionBlock;
        ISourceBlock<TOut> IStageOut<TOut>.ExecutionBlock => ExecutionBlock;
    }
}
