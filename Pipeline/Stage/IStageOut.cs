using System;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stage
{
    public interface IStageOut<TOut> : IStage
    {
        new ISourceBlock<PipelineItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);
       // new Func<StageCreationOptions, ISourceBlock<PipelineItem<TOut>>> CreateExecutionBlock { get; }

    }
}