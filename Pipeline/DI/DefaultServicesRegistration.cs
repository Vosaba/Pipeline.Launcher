//using EasyNetQ.ConnectionString;
//using EasyNetQ.Consumer;
//using EasyNetQ.Interception;
//using EasyNetQ.Producer;
//using EasyNetQ.Scheduling;
//using RabbitMQ.Client;

namespace PipelineLauncher.DI
{
    /// <summary>
    /// Registers the default EasyNetQ components
    /// </summary>
    public static class DefaultServicesRegistration
    {
        public static void RegisterDefaultServices(this IServiceRegister container)
        {
            Preconditions.CheckNotNull(container, "container");

            // Note: IConnectionConfiguration gets registered when RabbitHutch.CreateBus(..) is run.
            // default service registration
            //container
            //    .Register<IConsumerFactory, ConsumerFactory>()
            //    .Register(c =>
            //    {
            //        var connectionConfiguration = c.Resolve<ConnectionConfiguration>();
            //        return ConnectionFactoryFactory.CreateConnectionFactory(connectionConfiguration);
            //    })
            //    .Register<IClientCommandDispatcher, ClientCommandDispatcher>();
        }
    }
}
