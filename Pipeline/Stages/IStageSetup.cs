using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stages
{

    public interface IStageSetup
    {
        //StageType Type { get; }

        IStage Current { get; }
    }

    public interface IStageSetupIn<TInput> : IStageSetup
    {
        new IStageIn<TInput> Current { get; }
    }

    public interface IStageSetupOut<TOutput> : IStageSetup
    {
        new IStageOut<TOutput> Current { get; }
    }

    //public interface IStageSetup<TInput, TOutput> : IStageSetupIn<TInput>, IStageSetupOut<TOutput>
    //{
    //    new IStage<TInput, TOutput> Current { get; }
    //}
}