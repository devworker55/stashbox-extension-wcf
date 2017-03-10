using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Stashbox.Extension.Wcf
{
    public class StashboxDependencyInjectionOperationBehavior : IOperationBehavior, IDisposable
    {
        private Func<StashboxDependencyInjectionParameterInspector> _parameterInspectorFactory;

        public StashboxDependencyInjectionOperationBehavior(Func<StashboxDependencyInjectionParameterInspector> messageInspectorFactory)
        {
            _parameterInspectorFactory = messageInspectorFactory;
        }

        public void Validate(OperationDescription operationDescription)
        {
            // Method intentionally left empty.
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.ParameterInspectors.Add(_parameterInspectorFactory());
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            // Method intentionally left empty.
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
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
                    _parameterInspectorFactory = null;
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