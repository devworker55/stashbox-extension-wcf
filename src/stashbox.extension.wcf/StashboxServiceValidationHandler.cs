using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;

namespace Stashbox.Extension.Wcf
{
	public static class StashboxServiceValidationHandler
	{
		internal static void ValidateConcurrencyMode(List<Type> serviceTypes)
			=> serviceTypes.ForEach(ValidateConcurrencyMode);

		internal static void ValidateConcurrencyMode(Type serviceType)
		{
			if (HasInvalidConcurrencyMode(serviceType))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
					"The WCF service class {0} is configured with ConcurrencyMode Multiple, but this is not " +
					"supported by Stashbox. Please change the ConcurrencyMode to Single.", serviceType.FullName));
			}
		}

		internal static bool HasInvalidConcurrencyMode(Type serviceType) =>
			ServiceMetadataProvider.GetServiceBehaviorAttribute(serviceType)?.ConcurrencyMode == ConcurrencyMode.Multiple;
	}
}