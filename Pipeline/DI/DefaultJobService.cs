using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.LightInject;
using System.Reflection;

namespace PipelineLauncher.DI
{
    public class DefaultJobService : IJobService
    {
        private readonly ServiceContainer _container = new ServiceContainer();
        private bool _isAssemblyRegistered;

        public TPipelineJob GetJobInstance<TPipelineJob>() where TPipelineJob : class, IPipelineJob
        {
            if (!_isAssemblyRegistered)
            {
                _container.RegisterAssembly(Assembly.GetAssembly(typeof(TPipelineJob)));

                _isAssemblyRegistered = true;
            }

            return _container.GetInstance<TPipelineJob>();
        }
    }
}