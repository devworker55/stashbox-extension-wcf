using Stashbox.Entity;
using Stashbox.Infrastructure;
using Stashbox.Lifetime;
using System;
using System.Linq.Expressions;

namespace Stashbox.Extension.Wcf
{
    public sealed class PerServiceInstanceLifetime : LifetimeBase
    {
        private volatile Expression _expression;

        private readonly object _lock = new object();

        private readonly int _scopeId = Guid.NewGuid().GetHashCode();

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

        public override Expression GetExpression(IContainerContext containerContext, IObjectBuilder objectBuilder, ResolutionInfo resolutionInfo, Type resolveType)
        {
            if (this._expression != null) return this._expression;
            lock (this._lock)
            {
                if (this._expression != null) return this._expression;
                var expr = base.GetExpression(containerContext, objectBuilder, resolutionInfo, resolveType);
                if (expr == null)
                    return null;

                var factory = expr.CompileDelegate(Stashbox.Constants.ScopeExpression);

                var method = Constants.GetPerServiceInstanceScopedValueMethod.MakeGenericMethod(resolveType);

                this._expression = Expression.Call(method,
                    Stashbox.Constants.ScopeExpression,
                    Expression.Constant(factory),
                    Expression.Constant(this._scopeId));
            }

            return this._expression;
        }

        private static TValue CollectScopedInstance<TValue>(IResolutionScope scope, Func<IResolutionScope, object> factory, string scopeId)
            where TValue : class
        {
            var operationCtx = StashboxInstanceContext.Current;
            if (operationCtx == null)
                return null;

            if (operationCtx.Items[scopeId] != null)
                return operationCtx.Items[scopeId] as TValue;

            TValue instance = null;
            if (operationCtx.Scope is IResolutionScope resolutionScope)
            {
                instance = factory(resolutionScope) as TValue;
                operationCtx.Items[scopeId] = instance;
            }
            else
                instance = factory(scope) as TValue;

            return instance;
        }

        public override ILifetime Create() => new PerServiceInstanceLifetime(this._scopeId);
    }
}