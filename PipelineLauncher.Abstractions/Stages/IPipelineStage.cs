using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Abstractions.Stages
{
    public interface IPipelineStage
    {
    }

    public interface IPipelineStageIn<TInput>: IPipelineStage
    {
    }

    public interface IPipelineStageOut<TOutput>: IPipelineStage
    {
    }

    public interface IPipelineStage<TInput, TOutput>: IPipelineStageIn<TInput>, IPipelineStageOut<TOutput>
    {
    }

    public interface IConditionalStage<TInput> : IPipelineStageIn<TInput>
    {
        PredicateResult Predicate(TInput input);
    }
}
