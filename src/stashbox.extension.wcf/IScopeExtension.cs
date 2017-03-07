using Stashbox.Infrastructure;

namespace Stashbox.Extension.Wcf
{
    public interface IScopeExtension
    {
        IStashboxContainer Scope { get; }

        void TerminateScope();
    }
}