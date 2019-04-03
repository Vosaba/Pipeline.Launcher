namespace PipelineLauncher.Stages
{
    public interface IStageSetup<in TInput>
    {
        IStage<TInput> Current { get; }

    }// : IStageSetup


    public interface IStageSetup<TInput, TOutput> : IStageSetup<TInput>
    {
        new IStage<TInput, TOutput> Current { get; }
    }
}