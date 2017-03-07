using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace Stashbox.Extension.Wcf
{
    public class StashboxDependencyInjectionParameterInspector : IParameterInspector, IDisposable
    {
        private IScopeProvider _scopeProvider;

        public StashboxDependencyInjectionParameterInspector(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public virtual void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            if (StashboxConfig.EnablePerSericeOperationLifetime)
            {
                var operationContext = OperationContext.Current;
                operationContext?.Extensions.Find<StashboxOperationContext>()?.Dispose();
            }
        }

        public virtual object BeforeCall(string operationName, object[] inputs)
        {
            if (StashboxConfig.EnablePerSericeOperationLifetime)
            {
                var operationContext = OperationContext.Current;
                try
                {
                    var sbOperationContext = new StashboxOperationContext(_scopeProvider.GetOrCreateScope());

                    operationContext.Extensions.Add(sbOperationContext);
                    operationContext.OperationCompleted += (sender, e) =>
                    {
                        operationContext.Extensions.Find<StashboxOperationContext>()?.Dispose();
                    };
                }
                catch (Exception)
                {
                    operationContext.Extensions.Find<StashboxOperationContext>()?.Dispose();
                }
            }

            return "correlationState";
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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