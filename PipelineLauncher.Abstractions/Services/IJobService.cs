using PipelineLauncher.Abstractions.Pipeline;

namespace PipelineLauncher.Abstractions.Services
{
    public interface IJobService
    {
        TPipelineJob GetJobInstance<TPipelineJob>() where TPipelineJob : IPipelineJob;
    }
}