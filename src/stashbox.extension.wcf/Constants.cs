using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stashbox.Extension.Wcf
{
    internal static class Constants
    {
        internal static MethodInfo GetPerServiceInstanceScopedValueMethod = typeof(PerServiceInstanceLifetime).GetTypeInfo().GetDeclaredMethod("CollectScopedInstance");

        internal static MethodInfo GetPerServiceOperationScopedValueMethod = typeof(PerServiceOperationLifetime).GetTypeInfo().GetDeclaredMethod("CollectScopedInstance");
    }
}
