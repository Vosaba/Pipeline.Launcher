using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;
using System;

namespace PipelineLauncher.Demo.Tests.Fakes
{
    class FakeServicesRegistry
    {
        public class JobService : IJobService
        {
            public TPipelineJob GetJobInstance<TPipelineJob>() where TPipelineJob : class, IPipelineJob
            {
                return (TPipelineJob)Activator.CreateInstance(typeof(TPipelineJob));
            }
        }
    }
}
