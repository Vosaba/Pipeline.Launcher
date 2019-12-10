using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Stage;

namespace PipelineLauncher.PipelineSetup
{
    internal class FirstStageSetup<TFirstInput, TInput, TOutput> : PipelineSetup<TFirstInput, TOutput>, IStageSetupOut<TOutput>
    {
        private readonly IStage<TInput, TOutput> _stage;

        public FirstStageSetup(IStage<TInput, TOutput> stage, IJobService jobService) : base(stage, jobService)
        {
            _stage = stage;
        }

        IStageOut<TOutput> IStageSetupOut<TOutput>.Current => _stage;
    }
}
