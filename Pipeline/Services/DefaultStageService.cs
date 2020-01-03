using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.LightInject;
using System.Reflection;

namespace PipelineLauncher.Services
{
    public class DefaultStageService : IStageService
    {
        private readonly ServiceContainer _container = new ServiceContainer();
        private bool _isAssemblyRegistered;

        public TPipelineStage GetStageInstance<TPipelineStage>() where TPipelineStage : class, IStage
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