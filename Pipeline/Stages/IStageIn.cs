using PipelineLauncher.Abstractions.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.Stages
{
    public interface IStage
    {
        IDataflowBlock ExecutionBlock { get; }

        CancellationToken CancellationToken { get; }

        IList<IStage> Next { get; set; }

        IStage Previous { get; set; }
    }

    public interface IStageIn<TIn> : IStage
    {
        new ITargetBlock<PipelineItem<TIn>> ExecutionBlock { get; }
    }

    public interface IStageOut<TOut> : IStage
    {
        new ISourceBlock<PipelineItem<TOut>> ExecutionBlock { get; }
    }

    public interface IStage<TIn, TOut> : IStageIn<TIn>, IStageOut<TOut>
    {
        new IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> ExecutionBlock { get; }
    }
}
