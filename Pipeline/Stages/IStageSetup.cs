namespace PipelineLauncher.Stages
{

    public interface IStageSetupIn<TInput>
    {
        IStageIn<TInput> Current { get; }
    }

    public interface IStageSetupOut<TOutput>
    {
        IStageOut<TOutput> Current { get; }
    }

    public interface IStageSetup<TInput, TOutput> : IStageSetupIn<TInput>, IStageSetupOut<TOutput>
    {
        new IStage<TInput, TOutput> Current { get; }
    }
}