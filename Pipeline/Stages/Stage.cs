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
        public IStage Next { get; set; }
        public IStage Previous { get; set; }
        ITarget<TIn> IStage<TIn>.ExecutionBlock => ExecutionBlock;
    }
}
