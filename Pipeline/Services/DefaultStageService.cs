using System;
using System.Collections.Generic;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.Abstractions.Stages;
using PipelineLauncher.LightInject;
using System.Reflection;

namespace PipelineLauncher.Services
{
    public class DefaultStageService : IStageService
    {
        private readonly ServiceContainer _container = new ServiceContainer();
        private readonly List<string> _registeredAssemblies = new List<string>();

        public TPipelineStage GetStageInstance<TPipelineStage>() where TPipelineStage : class, IPipelineStage
        {
            try
            {
                return _container.GetInstance<TPipelineStage>();
            }
            catch (InvalidOperationException ex)
            {
                RegisterAssemblyForTypeWithSubTypesAssemblies(typeof(TPipelineStage));
            }
            
            return _container.GetInstance<TPipelineStage>();
        }

        private void RegisterAssemblyForTypeWithSubTypesAssemblies(Type type)
        {
            var assembly = Assembly.GetAssembly(type);

            if (!_registeredAssemblies.Contains(assembly.FullName))
            {
                _container.RegisterAssembly(assembly);
                _registeredAssemblies.Add(assembly.FullName);
            }

            ConstructorInfo[] constructorInfos = type.GetConstructors();

            foreach (var constructorInfo in constructorInfos)
            {
                var parameters = constructorInfo.GetParameters();
                foreach (var parameter in parameters)
                {
                    RegisterAssemblyForTypeWithSubTypesAssemblies(parameter.ParameterType);
                }
            }
        }
    }
}