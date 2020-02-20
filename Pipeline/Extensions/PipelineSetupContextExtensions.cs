using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.PipelineSetup;
using System;

namespace PipelineLauncher.Extensions
{
    public static class PipelineSetupContextExtensions
    {
        public static IPipelineSetup<TInput, TNextStageOutput> ExtensionContext<TInput, TOutput, TNextStageOutput>(this IPipelineSetup<TInput, TOutput> pipelineSetup, Func<IPipelineSetupOut<TOutput>, IPipelineSetupOut<TNextStageOutput>> extension)
        {
            return (IPipelineSetup<TInput, TNextStageOutput>)extension(pipelineSetup);
        }

        public static IPipelineSetupOut<TCast> Cast<TOutput, TCast>(this IPipelineSetupOut<TOutput> pipelineSetup)
            where TCast : class
        {
            return pipelineSetup.Stage(output => output as TCast);
        }

        public static IStageService AccessStageService<TInput, TOutput>(this IPipelineCreator pipelineCreator)
        {
            var ty = pipelineCreator.GetType();
            var pi = ty.GetProperty(nameof(PipelineCreationContext.StageService), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);
            object o = pi.GetValue(pipelineCreator, null);

            return (IStageService)o;
        }
    }
}