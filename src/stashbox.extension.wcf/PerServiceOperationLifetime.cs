using Stashbox.Entity;
using Stashbox.Infrastructure;
using Stashbox.Lifetime;
using Stashbox.Utils;
using System;
using System.Linq.Expressions;

namespace Stashbox.Extension.Wcf
{
    public sealed class PerServiceOperationLifetime : LifetimeBase
    {
        private readonly object _lock = new object();

        private readonly int _scopeId = Guid.NewGuid().GetHashCode();

        private StashboxOperationContext OperationContext => StashboxOperationContext.Current;

        public override bool IsScoped => true;

        /// <summary>
        /// Constructs a <see cref="PerServiceOperationLifetime"/>.
        /// </summary>
        public PerServiceOperationLifetime()
        {
            Shield.EnsureTrue(StashboxConfig.EnablePerSericeOperationLifetime, $"The {nameof(StashboxConfig)}.{nameof(StashboxConfig.EnablePerSericeOperationLifetime)} property must be set to true in order to use this lifetime.");
        }

        public PerServiceOperationLifetime(int scopedId) : this()
        {
            this._scopeId = scopedId;
        }

        public override Expression GetExpression(IContainerContext containerContext, IObjectBuilder objectBuilder, ResolutionInfo resolutionInfo, TypeInformation resolveType)
        {
            var call = Expression.Call(Expression.Constant(this), nameof(GetScopedInstance), null,
                                       Expression.Constant(containerContext),
                                       Expression.Constant(objectBuilder),
                                       Expression.Constant(resolutionInfo),
                                       Expression.Constant(resolveType));
            return Expression.Convert(call, resolveType.Type);
        }

        private object GetScopedInstance(IContainerContext containerContext, IObjectBuilder objectBuilder, ResolutionInfo resolutionInfo, TypeInformation resolveType)
        {
            lock (this._lock)
            {
                var operationCtx = OperationContext;
                if (operationCtx == null)
                    return null;

                if (operationCtx.Items[this._scopeId] != null)
                    return operationCtx.Items[this._scopeId];

                var scope = OperationContext.Scope;
                object instance = null;
                if (containerContext.Container == scope)
                {
                    var expr = base.GetExpression(containerContext, objectBuilder, resolutionInfo, resolveType);
                    instance = Expression.Lambda<Func<object>>(expr).Compile().Invoke();
                }
                else if (scope != null)
                    instance = scope.ActivationContext.Activate(resolutionInfo, resolveType);

                operationCtx.Items[this._scopeId] = instance;

                return instance;
            }
        }

        public override ILifetime Create() => new PerServiceOperationLifetime(this._scopeId);

        public override void CleanUp()
        {
            lock (this._lock)
            {
                var operationCtx = OperationContext;
                if (operationCtx == null)
                    return;

                if (operationCtx.Items[this._scopeId] != null)
                {
                    var instance = operationCtx.Items[this._scopeId] as IDisposable;
                    if (instance != null)
                    {
                        instance.Dispose();
                        operationCtx.Items[this._scopeId] = null;
                    }
                }
            }
        }
    }
}