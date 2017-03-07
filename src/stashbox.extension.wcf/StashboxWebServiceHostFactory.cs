using System;
using System.ServiceModel;

namespace Stashbox.Extension.Wcf
{
	public class StashboxWebServiceHostFactory : StashboxServiceHostFactoryBase
	{
		protected override ServiceHost CreateDefaultServiceHost(ServiceMetadata serviceMetadata, Uri[] baseAddresses)
		{
			if (serviceMetadata == null)
				throw new ArgumentNullException(nameof(serviceMetadata));

			if (baseAddresses == null)
				throw new ArgumentNullException(nameof(baseAddresses));

			return new StashboxWebServiceHost(serviceMetadata.ServiceType, baseAddresses);
		}

		protected override ServiceHost CreateSingletonServiceHost(ServiceMetadata serviceMetadata, Uri[] baseAddresses)
		{
			if (serviceMetadata == null)
				throw new ArgumentNullException(nameof(serviceMetadata));

			if (baseAddresses == null)
				throw new ArgumentNullException(nameof(baseAddresses));

			return new StashboxWebServiceHost(serviceMetadata.ServiceActivator.Invoke(), baseAddresses);
		}
	}
}