using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.PipelineStage
{
    public abstract class PipelineBaseStage<TInput, TOutput>:  IStage<TInput, TOutput>
    {
    }
}