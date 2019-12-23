using Autofac;
using Autofac.Features.ResolveAnything;
using PipelineLauncher.Abstractions.Pipeline;
using PipelineLauncher.Abstractions.Services;
using System;
using Autofac.Builder;

namespace PipelineLauncher.DI
{
    public class DefaultJobService : IJobService
    {
        private readonly DefaultServiceContainer _defaultServiceContainer = new DefaultServiceContainer();
        private readonly ContainerBuilder _containerBuilder = new ContainerBuilder();
        private readonly IContainer _container;

        public DefaultJobService()
        {
            _defaultServiceContainer.RegisterDefaultServices();

            _containerBuilder.RegisterAssemblyModules(AppDomain.CurrentDomain.GetAssemblies());
            _containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource()
            {
                RegistrationConfiguration =
                    (IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrationBuilder) =>
                    {
                        //registrationBuilder
                    }
            });

            _container = _containerBuilder.Build();
        }

        public TPipelineJob GetJobInstance<TPipelineJob>() where TPipelineJob : class, IPipelineJob
        {
            return _container.Resolve<TPipelineJob>();
        }
    }
}
