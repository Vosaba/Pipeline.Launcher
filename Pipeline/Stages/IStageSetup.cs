using PipelineLauncher.Abstractions.Dto;

namespace PipelineLauncher.Stages
{

    internal interface IStageSetup
    {
        IStage Current { get; }
    }

    internal interface IStageSetupIn<TInput> : IStageSetup
    {
        new IStageIn<TInput> Current { get; }
    }

    internal interface IStageSetupOut<TOutput> : IStageSetup
    {
        new IStageOut<TOutput> Current { get; }
    }

    //public interface IStageSetup<TInput, TOutput> : IStageSetupIn<TInput>, IStageSetupOut<TOutput>
    //{
    //    new IStage<TInput, TOutput> Current { get; }
    //}
}