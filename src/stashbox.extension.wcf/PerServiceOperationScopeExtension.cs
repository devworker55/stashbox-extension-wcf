using Stashbox.Infrastructure;
using System.ServiceModel;

namespace Stashbox.Extension.Wcf
{
    public class PerServiceOperationScopeExtension : IExtension<OperationContext>, IScopeExtension
    {
        public IStashboxContainer Scope { get; }

        public PerServiceOperationScopeExtension(IStashboxContainer scope)
        {
            Scope = scope;
        }

        public void Attach(OperationContext owner)
        {
            // Method intentionally left empty.
        }

        public void Detach(OperationContext owner)
        {
            // Method intentionally left empty.
        }

        public void TerminateScope()
        {
            Scope.Dispose();
        }
    }
}
