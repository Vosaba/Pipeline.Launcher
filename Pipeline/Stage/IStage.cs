using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stage
{
    public interface IStage<TIn, TOut> : IStageIn<TIn>, IStageOut<TOut>
    {
        new IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);
    }

    public interface IStage
    {
        IDataflowBlock RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);

        PipelineBaseConfiguration PipelineBaseConfiguration { get; }

        IList<IStage> Next { get; set; }

        IStage Previous { get; set; }

        void DestroyBlock();
    }
}