using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Extensions;
using PipelineLauncher.PipelineSetup;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests
{
    public static class Extensions
    {
        public static IPipelineSetupOut<int> MssCall(this IPipelineSetupOut<Item> pipelineSetup, string someValue)
        {
            return pipelineSetup.Stage(e => e.GetHashCode());
        }

        public static IPipelineSetupOut<TOutput> TestStage<TStage, TInput, TOutput>(this IPipelineSetupOut<TInput> pipelineSetup)
            where TStage : Stage<TInput, TOutput>
        {
            var t = pipelineSetup.AccessStageService();

            return pipelineSetup.Stage(t.GetStageInstance<TStage>());
        }
    }
}
