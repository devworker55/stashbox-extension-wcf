using Stashbox.Entity;
using Stashbox.Infrastructure;
using Stashbox.Lifetime;
using System;
using System.Linq.Expressions;

namespace Stashbox.Extension.Wcf
{
    public sealed class PerServiceInstanceLifetime : LifetimeBase
    {
        private readonly object _lock = new object();

        private readonly int _scopeId = Guid.NewGuid().GetHashCode();

        private StashboxInstanceContext InstanceContext => StashboxInstanceContext.Current;

        public override bool IsScoped => true;

        /// <summary>
        /// Constructs a <see cref="PerServiceInstanceLifetime"/>.
        /// </summary>
        public PerServiceInstanceLifetime() { }

        /// <summary>
        /// Constructs a <see cref="PerServiceInstanceLifetime"/>.
        /// </summary>
        /// <param name="scopedId"></param>
        public PerServiceInstanceLifetime(int scopedId)
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
                var instanceCtx = InstanceContext;
                if (instanceCtx == null)
                    return null;

                if (instanceCtx.Items[this._scopeId] != null)
                    return instanceCtx.Items[this._scopeId];

                var scope = InstanceContext.Scope;
                object instance = null;
                if (containerContext.Container == scope)
                {
                    var expr = base.GetExpression(containerContext, objectBuilder, resolutionInfo, resolveType);
                    instance = Expression.Lambda<Func<object>>(expr).Compile().Invoke();
                }
                else if (scope != null)
                    instance = scope.ActivationContext.Activate(resolutionInfo, resolveType);

                instanceCtx.Items[this._scopeId] = instance;

                return instance;
            }
        }

        public override ILifetime Create() => new PerServiceInstanceLifetime(this._scopeId);

        public override void CleanUp()
        {
            lock (this._lock)
            {
                var instanceCtx = InstanceContext;
                if (instanceCtx == null)
                    return;

                if (instanceCtx.Items[this._scopeId] != null)
                {
                    var instance = instanceCtx.Items[this._scopeId] as IDisposable;
                    if (instance != null)
                    {
                        instance.Dispose();
                        instanceCtx.Items[this._scopeId] = null;
                    }
                }
            }
        }
    }
}