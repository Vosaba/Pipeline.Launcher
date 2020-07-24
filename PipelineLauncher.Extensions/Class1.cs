using System;
using PipelineLauncher.PipelineSetup;

namespace PipelineLauncher.Extensions
{
    public static class StageExtensions
    {
        public static void ConfigurableStage<TInput, TOutput, TNextOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<TOutput, TNextOutput> stage)
        {
        }
    }
}
