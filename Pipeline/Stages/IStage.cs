using System.Collections.Generic;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Dataflow;

namespace PipelineLauncher.Stages
{
    public interface IStage
    {
        IStage Next { get; set; }

        IStage Previous { get; set; }
    }

    public interface IStage<in TIn> : IStage
    {
        ITarget<TIn> ExecutionBlock { get; }
    }

    public interface IStage<TIn, TOut> : IStage<TIn>
    {
        new ITarget<TIn, TOut> ExecutionBlock { get; }
    }
}
