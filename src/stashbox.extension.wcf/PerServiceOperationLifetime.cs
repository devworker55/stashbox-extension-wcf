using Stashbox.Entity;
using Stashbox.Infrastructure;
using Stashbox.Lifetime;
using Stashbox.Utils;
using System;
using System.Linq.Expressions;
using Stashbox.Infrastructure.Registration;

namespace Stashbox.Extension.Wcf
{
    public sealed class PerServiceOperationLifetime : LifetimeBase
    {
        private volatile Expression _expression;

        private readonly object _lock = new object();

        private readonly int _scopeId = Guid.NewGuid().GetHashCode();

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

        public override Expression GetExpression(IServiceRegistration serviceRegistration, IObjectBuilder objectBuilder, ResolutionInfo resolutionInfo, Type resolveType)
        {
            if (this._expression != null) return this._expression;
            lock (this._lock)
            {
                if (this._expression != null) return this._expression;
                var expr = base.GetExpression(serviceRegistration, objectBuilder, resolutionInfo, resolveType);
                if (expr == null)
                    return null;

                var factory = expr.CompileDelegate(Stashbox.Constants.ScopeExpression);

                var method = Constants.GetPerServiceOperationScopedValueMethod.MakeGenericMethod(resolveType);

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
            var operationCtx = StashboxOperationContext.Current;
            if (operationCtx == null)
                return null;

            if (operationCtx.Items[scopeId] != null)
                return operationCtx.Items[scopeId] as TValue;

            TValue instance;
            var resolutionScope = operationCtx.Scope as IResolutionScope;
            if (resolutionScope != null)
            {
                instance = factory(resolutionScope) as TValue;
                operationCtx.Items[scopeId] = instance;
            }
            else
                instance = factory(scope) as TValue;

            return instance;
        }

        public override ILifetime Create() => new PerServiceOperationLifetime(this._scopeId);
    }
}