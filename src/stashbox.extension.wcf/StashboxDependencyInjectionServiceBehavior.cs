using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Stashbox.Extension.Wcf
{
    public class StashboxDependencyInjectionServiceBehavior : IServiceBehavior, IDisposable
    {
        private Func<StashboxInstanceProvider> _instanceProviderFactory;

        public StashboxDependencyInjectionServiceBehavior(Func<StashboxInstanceProvider> instanceProviderFactory)
        {
            _instanceProviderFactory = instanceProviderFactory;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            // Method intentionally left empty.
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (var endpoinDispatcher in channelDispatcher.Endpoints)
                    {
                        endpoinDispatcher.DispatchRuntime.InstanceProvider = _instanceProviderFactory();
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // Method intentionally left empty.
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _instanceProviderFactory = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}