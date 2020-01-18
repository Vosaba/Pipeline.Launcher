using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.PipelineStage.Configurations;
using PipelineLauncher.Abstractions.PipelineStage.Dto;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetup<TIn, TOut> : IStageSetupIn<TIn>, IStageSetupOut<TOut>
    {
        new IPropagatorBlock<PipelineStageItem<TIn>, PipelineStageItem<TOut>> RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);
    }

    public interface IStageSetup
    {
        IDataflowBlock RetrieveExecutionBlock(StageCreationOptions options, bool forceCreation = false);

        StageBaseConfiguration PipelineBaseConfiguration { get; }

        IList<IStageSetup> Next { get; set; }

        IStageSetup Previous { get; set; }

        void DestroyBlock();
    }
}