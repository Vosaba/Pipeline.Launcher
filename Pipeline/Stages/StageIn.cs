using System.Collections.Generic;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Dataflow;

namespace PipelineLauncher.Stages
{
    internal class Stage<TIn, TOut> : IStage<TIn, TOut>
    {
        public Stage(ITarget<TIn, TOut> executionBlock)
        {
            ExecutionBlock = executionBlock;
        }

        public ITarget<TIn, TOut> ExecutionBlock { get; }
        public IStageIn<TOut> Next { get; set; }
        public IStageOut<TIn> Previous { get; set; }

        ITargetIn<TIn> IStageIn<TIn>.ExecutionBlock => ExecutionBlock;
        ITargetOut<TOut> IStageOut<TOut>.ExecutionBlock => ExecutionBlock;

        IStage IStage.Previous => Previous;
        IStage IStage.Next => Next;


    }
}
