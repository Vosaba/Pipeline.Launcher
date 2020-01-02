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
}
