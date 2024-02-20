using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Abstractions.PipelineStage
{
    public interface IPipelineStage
    {
    }

    public interface IPipelineStageTarget<TInput>: IPipelineStage
    {
    }

    public interface IPipelineStageSource<TOutput>: IPipelineStage
    {
    }

    public interface IPipelineStage<TInput, TOutput>: IPipelineStageTarget<TInput>, IPipelineStageSource<TOutput>
    {
    }

    public interface IConditionalStage<TInput> : IPipelineStageTarget<TInput>
    {
        PredicateResult Predicate(TInput input);
    }
}
