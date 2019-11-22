using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Pipeline;
//using PipelineLauncher.Dataflow;

namespace PipelineLauncher.Stages
{
    internal class Stage<TIn, TOut> : IStage<TIn, TOut>
    {
        public Stage(IPropagatorBlock<TIn, TOut> executionBlock)
        {
            ExecutionBlock = executionBlock;
        }

        public IPropagatorBlock<TIn, TOut> ExecutionBlock { get; }
        public IStageIn<TOut> Next { get; set; }
        public IStageOut<TIn> Previous { get; set; }

        ITargetBlock<TIn> IStageIn<TIn>.ExecutionBlock => ExecutionBlock;
        ISourceBlock<TOut> IStageOut<TOut>.ExecutionBlock => ExecutionBlock;

        IStage IStage.Previous => Previous;
        IStage IStage.Next => Next;


    }
}
