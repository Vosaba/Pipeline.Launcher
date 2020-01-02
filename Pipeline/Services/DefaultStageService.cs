using System.Reflection;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.LightInject;

namespace PipelineLauncher.Services
{
    public class DefaultStageService : IStageService
    {
        private readonly ServiceContainer _container = new ServiceContainer();
        private bool _isAssemblyRegistered;

        public TPipelineStage GetStageInstance<TPipelineStage>() where TPipelineStage : class, IPipeline
        {
            if (!_isAssemblyRegistered)
            {
                _container.RegisterAssembly(Assembly.GetAssembly(typeof(TPipelineStage)));

                _isAssemblyRegistered = true;
            }

            return _container.GetInstance<TPipelineStage>();
        }
    }
}