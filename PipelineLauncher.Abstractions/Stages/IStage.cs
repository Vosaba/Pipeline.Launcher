using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Abstractions.Stages
{
    public interface IStage
    {
    }

    public interface IStageIn<TInput>: IStage
    {
    }

    public interface IStageOut<TOutput>: IStage
    {
    }

    public interface IStage<TInput, TOutput>: IStageIn<TInput>, IStageOut<TOutput>
    {
    }

    public interface IConditionalStage<TInput> : IStageIn<TInput>
    {
        PredicateResult Predicate(TInput input);
    }
}
