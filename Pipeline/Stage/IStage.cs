using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stage
{
    public interface IStage<TIn, TOut> : IStageIn<TIn>, IStageOut<TOut>
    {
        new IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> ExecutionBlock { get; }
    }

    public interface IStage
    {
        Func<IDataflowBlock> CreateBlock { get; }

        IDataflowBlock ExecutionBlock { get; }

        CancellationToken CancellationToken { get; }

        IList<IStage> Next { get; set; }

        IStage Previous { get; set; }

        void DestroyBlock();
    }
}