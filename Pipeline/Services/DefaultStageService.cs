using System;
using System.Collections.Generic;
using PipelineLauncher.Abstractions.Services;
using PipelineLauncher.LightInject;
using System.Reflection;
using PipelineLauncher.Abstractions.PipelineStage;
using PipelineLauncher.Abstractions.Stages;

namespace PipelineLauncher.Services
{
    public class DefaultStageService : IStageService
    {
        private readonly ServiceContainer _container = new ServiceContainer();
        private readonly List<string> _registeredAssemblies = new List<string>();

        public TStage GetStageInstance<TStage>() where TStage : class, IStage
        {
            try
            {
                return _container.GetInstance<TStage>();
            }
            catch (InvalidOperationException ex)
            {
                RegisterAssemblyForTypeWithSubTypesAssemblies(typeof(TStage));
            }
            
            return _container.GetInstance<TStage>();
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