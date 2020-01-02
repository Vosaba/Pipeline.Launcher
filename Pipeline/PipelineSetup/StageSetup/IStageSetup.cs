using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Configurations;
using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetup<TIn, TOut> : IStageSetupIn<TIn>, IStageSetupOut<TOut>
    {
        new IPropagatorBlock<PipelineItem<TIn>, PipelineItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);
    }

    public interface IStageSetup
    {
        IDataflowBlock RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);

        PipelineBaseConfiguration PipelineBaseConfiguration { get; }

        IList<IStageSetup> Next { get; set; }

        IStageSetup Previous { get; set; }

        void DestroyBlock();
    }
}