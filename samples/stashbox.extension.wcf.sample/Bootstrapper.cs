using stashbox.extension.wcf.sample.domain;
using Stashbox;
using Stashbox.Extension.Wcf;
using Stashbox.Infrastructure;
using System;
using System.Reflection;
using System.ServiceModel;

namespace stashbox.extension.wcf.sample
{
    public static class Bootstrapper
    {
        public static IStashboxContainer Configure()
        {
            //-> Use when using an InstanceContextMode other than "PerCall",
            //   requires implementation in combination with ChildScopes in order to achieve a PerScopeLifetime.
            //   See "SingletonService" sample e.g for implementation reference.
            //   N.B - Setting this to true will implicitly enable PerServiceOperationLifetime for all services, adds no side-effects however.
            StashboxConfig.EnablePerSericeOperationLifetime = false;

            //-> Use when allowing the extension to register all discoverable wcf services on your behalf.
            //   Sets the default lifetime that the services should be configured with upon registration.
            //   A value of "Scoped" registers each service with the PerServicveInstanceLifetime.
            StashboxConfig.DefaultServiceLifetime = ServiceRegistrationLifetime.Scoped;

            //-> Use when allowing the extension to dynamically register all discoverable wcf services
            //   or any un-registered service type declared in any of ".svc" files.
            StashboxConfig.ServiceAssemblies = new Assembly[] { Assembly.GetExecutingAssembly() };


            var preConfiguredContainer = StashboxConfig.RegisterStashbox(cont =>
            {
                cont.RegisterWcfServices(Assembly.GetExecutingAssembly());
                cont.RegisterType<IUserRepository, UserRepository>();
                cont.RegisterType<IProductRepository, ProductRepository>();
            });

            //-> Or

            var container = new StashboxContainer(config => config.WithUnknownTypeResolution().WithCircularDependencyTracking());
            container.RegisterType<SingletonService>();
            container.RegisterType<DefaultSampleService>();
            container.RegisterType<IUserRepository, UserRepository>();
            container.RegisterType<IProductRepository, ProductRepository>();
            StashboxConfig.RegisterStashbox(container);


            //-> To add targeted behavior to a specific service, use the below hook.
            StashboxServiceHostFactoryBase.RegisterOnHostCreatedAction(typeof(SingletonService), host =>
            {
                //-> This e.g enables PerServiceOperationLifetime only for the SingletonService.
                (host as ServiceHost)?.AddOperationBehavior(
                    new StashboxDependencyInjectionOperationBehavior(
                        container.Resolve<Func<StashboxDependencyInjectionParameterInspector>>()));
            });

            return container ?? preConfiguredContainer;
        }
    }
}
