using System;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;

namespace PipelineLauncher.DI
{
    public class DefaultLambdaJobService : IJobService
    {
        private Func<Type, IPipelineJob> _jobResolver;

        public DefaultLambdaJobService(Func<Type, IPipelineJob> jobResolver)
        {
            _jobResolver = jobResolver;
        }

        public TPipelineJob GetJobInstance<TPipelineJob>() where TPipelineJob : class, IPipelineJob
        {
            return (TPipelineJob) _jobResolver(typeof(TPipelineJob));
        }
    }
}