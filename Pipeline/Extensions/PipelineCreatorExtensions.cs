using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.PipelineSetup;

namespace PipelineLauncher.Extensions
{
    public static class PipelineCreatorExtensions
    {
        public static IStageService AccessStageService<TInput, TOutput>(this IPipelineCreator pipelineCreator)
        {
            var ty = pipelineCreator.GetType();
            var pi = ty.GetProperty(nameof(PipelineCreationContext.StageService), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);
            object o = pi.GetValue(pipelineCreator, null);

            return (IStageService)o;
        }
    }
}