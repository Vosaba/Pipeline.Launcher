using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Pipelines;

namespace PipelineLauncher.Extensions
{
    public static class PipelineCreatorExtensions
    {
        public static IJobService AccessJobService<TInput, TOutput>(this IPipelineCreator pipelineCreator)
        {
            var ty = pipelineCreator.GetType();
            var pi = ty.GetProperty("JobService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);
            object o = pi.GetValue(pipelineCreator, null);

            return (IJobService)o;
        }
    }
}