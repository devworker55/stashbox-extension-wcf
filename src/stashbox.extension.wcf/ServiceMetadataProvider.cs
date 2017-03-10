using Stashbox.Infrastructure;
using Stashbox.Infrastructure.Registration;
using Stashbox.Lifetime;
using Stashbox.Utils;
using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Stashbox.Extension.Wcf
{
    public static class ServiceMetadataProvider
    {
        public static ServiceMetadata GetMetadata(Type serviceType)
        {
            var serviceRegistrationInfo = GetOrCreateRegistration(serviceType);
            var serviceLifetime = GetRegistrationLifetime(serviceRegistrationInfo, serviceType);
            var serviceBehaviorAttribute = GetServiceBehaviorAttribute(serviceType);
            var aspNetCompatibilityRequirementsAttribute = GetAspNetCompatibilityRequirementsAttribute(serviceType);
            var serviceActivator = serviceLifetime == ServiceRegistrationLifetime.Singleton ? (Func<object>)(() => StashboxServiceHostFactoryBase.Container.Resolve(serviceType))
                                                                                            : (Func<object>)(() => default(object));
            return new ServiceMetadata(serviceType,
                                       serviceActivator,
                                       serviceLifetime,
                                       serviceRegistrationInfo,
                                       serviceBehaviorAttribute,
                                       aspNetCompatibilityRequirementsAttribute);
        }

        public static bool IsWcfServiceType(Type type) => IsServiceContract(type) || IsConcreteService(type);

        private static bool IsServiceContract(Type type) => type.GetCustomAttributes(typeof(ServiceContractAttribute), true).Any();

        private static bool IsConcreteService(Type type) => (from @interface in type.GetInterfaces()
                                                             where @interface.IsPublic
                                                             where @interface.GetCustomAttributes(typeof(ServiceContractAttribute), true).Any()
                                                             select @interface).Any();

        private static Type[] GetConcreteServiceTypes(Type serivceContractType) => StashboxConfig.ServiceAssemblies?
                                                                                                 .SelectMany(s => s.GetTypes())
                                                                                                 .Where(serivceContractType.IsAssignableFrom)
                                                                                                 .ToArray();

        internal static IServiceRegistration GetRegistration(Type serviceType) =>
            StashboxServiceHostFactoryBase.Container.ContainerContext
                                                    .RegistrationRepository
                                                    .GetAllRegistrations()
                                                    .FirstOrDefault(reg => reg.ServiceType == serviceType);

        internal static bool IsRegistered(this Type serviceType) => GetRegistration(serviceType) != null;

        internal static AspNetCompatibilityRequirementsAttribute GetAspNetCompatibilityRequirementsAttribute(Type serviceType) =>
                    GetAttribute<AspNetCompatibilityRequirementsAttribute>(serviceType);

        internal static ServiceBehaviorAttribute GetServiceBehaviorAttribute(Type serviceType) =>
                    GetAttribute<ServiceBehaviorAttribute>(serviceType);

        internal static TAttribute GetAttribute<TAttribute>(Type type) where TAttribute : Attribute =>
                        type.GetCustomAttributes(typeof(TAttribute), true)
                            .OfType<TAttribute>()
                            .FirstOrDefault();

        internal static ServiceRegistrationLifetime GetRegistrationLifetime(IServiceRegistration serviceRegistrationInfo, Type serviceType)
        {
            Shield.EnsureNotNull(serviceRegistrationInfo, nameof(serviceRegistrationInfo));

            ServiceRegistrationLifetime? registrationLifetime = null;
            var lifetime = serviceRegistrationInfo.LifetimeManager;

            if (lifetime is TransientLifetime || lifetime.IsTransient)
                registrationLifetime = ServiceRegistrationLifetime.Transient;
            if (lifetime is SingletonLifetime)
                registrationLifetime = ServiceRegistrationLifetime.Singleton;
            if (lifetime.IsScoped)
                registrationLifetime = ServiceRegistrationLifetime.Scoped;

            Shield.EnsureTrue(registrationLifetime.HasValue, $"An unsupported lifetime of type \"{lifetime.GetType().FullName}\" has been detected for service type \"{serviceType.FullName}\".");

            return registrationLifetime.Value;
        }

        internal static IServiceRegistration GetOrCreateRegistration(Type serviceType)
        {
            var container = StashboxServiceHostFactoryBase.Container;
            var serviceRegistrationInfo = GetRegistration(serviceType);
            var unknownTypeResolutionEnabled = container.ContainerContext
                                                        .ContainerConfigurator
                                                        .ContainerConfiguration
                                                        .UnknownTypeResolutionEnabled;

            if (serviceRegistrationInfo == null)
            {
                Shield.EnsureTrue(unknownTypeResolutionEnabled, $"The given service type \"{serviceType.FullName}\" has not been registered with the container and the container configuration setting \"UnknownTypeResolutionEnabled\" is not set to true.");

                if (IsConcreteService(serviceType))
                {
                    container.PrepareType(serviceType).WithLifetime(GetLifetimeScope(serviceType)).Register();
                }
                else if (IsServiceContract(serviceType))
                {
                    var concreteServiceTypes = GetConcreteServiceTypes(serviceType);
                    Shield.EnsureNotNull(concreteServiceTypes, $"A concrete service type that implements \"{serviceType.FullName}\" could not be found in the list of known service assemblies defined by \"{nameof(StashboxConfig)}.{nameof(StashboxConfig.ServiceAssemblies)}\".");
                    foreach (var concreteServiceType in concreteServiceTypes)
                    {
                        container.PrepareType(concreteServiceType).WithLifetime(GetLifetimeScope(concreteServiceType)).Register();
                    }
                }
                else
                {
                    throw new InvalidOperationException($"The given service type \"{serviceType.FullName}\" is an invalid service type, it should have either the \"ServiceContractAttribute\" declared " +
                                                         "on the type or implements an interface that has the \"ServiceContractAttribute\" declared on that interface type.");
                }
            }

            return serviceRegistrationInfo ?? GetRegistration(serviceType);
        }

        internal static ILifetime GetLifetimeScope(Type serviceType)
        {
            var lifetime = default(ILifetime);
            var isSingleton = GetServiceBehaviorAttribute(serviceType)?.InstanceContextMode == InstanceContextMode.Single;
            if (isSingleton)
                lifetime = new SingletonLifetime();
            else if (StashboxConfig.DefaultServiceLifetime.HasValue)
            {
                switch (StashboxConfig.DefaultServiceLifetime)
                {
                    case ServiceRegistrationLifetime.Transient:
                        lifetime = new TransientLifetime();
                        break;
                    case ServiceRegistrationLifetime.Singleton:
                        lifetime = new SingletonLifetime();
                        break;
                    case ServiceRegistrationLifetime.Scoped:
                        lifetime = new PerServiceInstanceLifetime();
                        break;
                }
            }
            else
            {
                lifetime = new TransientLifetime();
            }
            return lifetime;
        }
    }
}