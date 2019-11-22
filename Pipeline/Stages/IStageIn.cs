using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Pipeline;
//using PipelineLauncher.Dataflow;

namespace PipelineLauncher.Stages
{
    public interface IStage
    {
        IStage Next { get; }

        IStage Previous { get; }
    }

    public interface IStageIn<TIn> : IStage
    {
        ITargetBlock<TIn> ExecutionBlock { get; }
    }

    public interface IStageOut<TOut> : IStage
    {
        ISourceBlock<TOut> ExecutionBlock { get; }
    }

    public interface IStage<TIn, TOut> : IStageIn<TIn>, IStageOut<TOut>
    {
        new IPropagatorBlock<TIn, TOut> ExecutionBlock { get; }

        new IStageIn<TOut> Next { get; set; }

        new IStageOut<TIn> Previous { get; set; }
    }
}
