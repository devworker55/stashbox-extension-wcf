using Stashbox.Infrastructure.Registration;
using Stashbox.Utils;
using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Stashbox.Extension.Wcf
{
    public class ServiceMetadata
    {
        public Type ServiceType { get; }

        public Func<object> ServiceActivator { get; }

        public ServiceRegistrationLifetime ServiceLifetime { get; }

        internal IServiceRegistration RegistrationInformation { get; }

        public ServiceBehaviorAttribute ServiceBehaviorConfiguration { get; }

        public AspNetCompatibilityRequirementsAttribute AspNetCompatibilityRequirementsAttribute { get; }

        public string ServiceName =>
            ServiceBehaviorConfiguration.Name;

        public ConcurrencyMode ConcurrencyMode =>
            ServiceBehaviorConfiguration.ConcurrencyMode;

        public InstanceContextMode InstanceContextMode =>
            ServiceBehaviorConfiguration.InstanceContextMode;

        public AspNetCompatibilityRequirementsMode AspNetCompatibilityRequirementsMode
            => AspNetCompatibilityRequirementsAttribute.RequirementsMode;

        public bool IsSingletonConfiguration => InstanceContextMode == InstanceContextMode.Single && ServiceLifetime == ServiceRegistrationLifetime.Singleton;

        public ServiceMetadata(Type serviceType,
                               Func<object> serviceActivator,
                               ServiceRegistrationLifetime serviceRegistrationLifetime,
                               IServiceRegistration registrationInformation,
                               ServiceBehaviorAttribute serviceBehaviorConfig,
                               AspNetCompatibilityRequirementsAttribute compatibilityRequirements)
        {
            Shield.EnsureNotNull(serviceType, nameof(serviceType));
            Shield.EnsureNotNull(registrationInformation, nameof(registrationInformation));
            Shield.EnsureNotNull(serviceBehaviorConfig, nameof(serviceBehaviorConfig));
            Shield.EnsureNotNull(compatibilityRequirements, nameof(compatibilityRequirements));

            ServiceType = serviceType;
            ServiceActivator = serviceActivator ?? (() => null);
            ServiceLifetime = serviceRegistrationLifetime;
            RegistrationInformation = registrationInformation;
            ServiceBehaviorConfiguration = serviceBehaviorConfig;
            AspNetCompatibilityRequirementsAttribute = compatibilityRequirements;
        }
    }
}
