using System;

namespace Stashbox.Extension.Wcf
{
    public interface IScopeProvider : IDisposable
    {
        IScopeExtension GetOrCreateScope();
    }
}