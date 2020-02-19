using PipelineLauncher.Abstractions.Dto;
using PipelineLauncher.Abstractions.PipelineStage;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace PipelineLauncher.StageSetup
{
    public interface IStageSetup<TIn, TOut> : IStageSetupIn<TIn>, IStageSetupOut<TOut>
    {
        new IPropagatorBlock<PipelineStageItem<TIn>, PipelineStageItem<TOut>> RetrieveExecutionBlock(StageCreationContext context);
    }

    public interface IStageSetup
    {
        IDataflowBlock RetrieveExecutionBlock(StageCreationContext context);

        void DestroyExecutionBlock();

        IList<IStageSetup> Next { get; set; }

        IStageSetup Previous { get; set; }
    }
}