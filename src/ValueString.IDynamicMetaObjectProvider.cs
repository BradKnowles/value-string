// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#if DYNAMIC

namespace Dawn
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <content>Contains the <see cref="IDynamicMetaObjectProvider" /> implementation.</content>
    public partial struct ValueString : IDynamicMetaObjectProvider
    {
        /// <inheritdoc />
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
            => new ValueStringMetaObject(parameter, this);

        /// <summary>Represents the dynamic binding of a specific <see cref="ValueString" />.</summary>
        private sealed class ValueStringMetaObject : DynamicMetaObject
        {
            /// <summary>
            ///     Cached <see cref="MethodInfo" /> instances of <see cref="As{T}()" />.
            /// </summary>
            private static readonly TypeCache<MethodInfo> asMethods
                = new TypeCache<MethodInfo>();

            /// <summary>
            ///     Initializes a new instance of the <see cref="ValueStringMetaObject" /> class.
            /// </summary>
            /// <param name="expression">
            ///     The expression representing this <see cref="ValueStringMetaObject" />
            ///     during the dynamic binding process.
            /// </param>
            /// <param name="value">
            ///     The runtime value string represented by the
            ///     <see cref="ValueStringMetaObject" />.
            /// </param>
            public ValueStringMetaObject(Expression expression, ValueString value)
                : base(expression, BindingRestrictions.Empty, value)
            {
            }

            /// <inheritdoc />
            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                var restrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
                if (binder.Type == this.LimitType)
                    return binder.FallbackConvert(new DynamicMetaObject(this.Expression, restrictions, this.Value));

                var method = asMethods.GetOrAdd(binder.Type, t => GetAsMethod(t, false));
                var call = Expression.Call(Expression.Convert(this.Expression, this.LimitType), method);
                return new DynamicMetaObject(call, restrictions);
            }
        }
    }
}

#endif
