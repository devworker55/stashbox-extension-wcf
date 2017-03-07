using System;
using System.ServiceModel;

namespace Stashbox.Extension.Wcf
{
	public class StashboxServiceHostFactory : StashboxServiceHostFactoryBase
	{
		protected override ServiceHost CreateDefaultServiceHost(ServiceMetadata serviceMetadata, Uri[] baseAddresses)
		{
			if (serviceMetadata == null)
				throw new ArgumentNullException(nameof(serviceMetadata));

			if (baseAddresses == null)
				throw new ArgumentNullException(nameof(baseAddresses));

			return new StashboxServiceHost(serviceMetadata.ServiceType, baseAddresses);
		}

		protected override ServiceHost CreateSingletonServiceHost(ServiceMetadata serviceMetadata, Uri[] baseAddresses)
		{
			if (serviceMetadata == null)
				throw new ArgumentNullException(nameof(serviceMetadata));

			if (baseAddresses == null)
				throw new ArgumentNullException(nameof(baseAddresses));

			return new StashboxServiceHost(serviceMetadata.ServiceActivator.Invoke(), baseAddresses);
		}
	}
}