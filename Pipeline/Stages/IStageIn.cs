using System.Collections.Generic;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Dataflow;

namespace PipelineLauncher.Stages
{
    public interface IStage
    {
        IStage Next { get; }

        IStage Previous { get; }
    }

    public interface IStageIn<TIn> : IStage
    {
        ITargetIn<TIn> ExecutionBlock { get; }
    }

    public interface IStageOut<TOut> : IStage
    {
        ITargetOut<TOut> ExecutionBlock { get; }
    }

    public interface IStage<TIn, TOut> : IStageIn<TIn>, IStageOut<TOut>
    {
        new ITarget<TIn, TOut> ExecutionBlock { get; }

        new IStageIn<TOut> Next { get; set; }

        new IStageOut<TIn> Previous { get; set; }
    }
}
