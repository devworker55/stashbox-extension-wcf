using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Stashbox.Extension.Wcf
{
    public class StashboxInstanceProvider : IInstanceProvider, IDisposable
    {
        private IScopeProvider _scopeProvider;
        public StashboxInstanceProvider(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            try
            {
                var sbInstanceContext = new StashboxInstanceContext(_scopeProvider.GetOrCreateScope());
                var serviceInstance = sbInstanceContext.Scope.Resolve(instanceContext.Host.Description.ServiceType);

                instanceContext.Extensions.Add(sbInstanceContext);
                instanceContext.Closing += (sender, e) =>
                {
                    instanceContext.Extensions.Find<StashboxInstanceContext>()?.Dispose();
                };

                return serviceInstance;
            }
            catch (Exception)
            {
                // We need to dispose the scope here, because WCF will never call ReleaseInstance if
                // this method throws an exception (since it has no instance to pass to ReleaseInstance).
                instanceContext.Extensions.Find<StashboxInstanceContext>()?.Dispose();
                throw;
            }
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return GetInstance(instanceContext);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            if (instanceContext == null)
            {
                throw new ArgumentNullException(nameof(instanceContext));
            }

            instanceContext.Extensions.Find<StashboxInstanceContext>()?.Dispose();
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _scopeProvider.Dispose();
                    _scopeProvider = null;
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
