using PipelineLauncher.Demo.Tests.Items;
using PipelineLauncher.Extensions;
using PipelineLauncher.PipelineSetup;
using PipelineLauncher.Stages;

namespace PipelineLauncher.Demo.Tests
{
    public static class Extensions
    {
        public static IPipelineSetupSource<int> MssCall(this IPipelineSetupSource<Item> pipelineSetup, string someValue)
        {
            return pipelineSetup.Stage(e => e.GetHashCode());
        }

        public static IPipelineSetupSource<TOutput> TestStage<TStage, TInput, TOutput>(this IPipelineSetupSource<TInput> pipelineSetup)
            where TStage : Stage<TInput, TOutput>
        {
            var t = pipelineSetup.AccessStageService();

            return pipelineSetup.Stage(t.GetStageInstance<TStage>());
        }
    }
}
