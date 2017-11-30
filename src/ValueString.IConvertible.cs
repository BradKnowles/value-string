// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#if !NETSTANDARD1_0

namespace Dawn
{
    using System;
    using System.Globalization;

    /// <content>Contains the <see cref="IConvertible" /> implementation.</content>
    public partial struct ValueString : IConvertible
    {
        /// <inheritdoc />
        TypeCode IConvertible.GetTypeCode()
            => TypeCode.Object;

        /// <inheritdoc />
        bool IConvertible.ToBoolean(IFormatProvider provider)
            => this.As<bool>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        byte IConvertible.ToByte(IFormatProvider provider)
            => this.As<byte>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        char IConvertible.ToChar(IFormatProvider provider)
            => this.As<char>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        DateTime IConvertible.ToDateTime(IFormatProvider provider)
            => this.As<DateTime>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        decimal IConvertible.ToDecimal(IFormatProvider provider)
            => this.As<decimal>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        double IConvertible.ToDouble(IFormatProvider provider)
            => this.As<double>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        short IConvertible.ToInt16(IFormatProvider provider)
            => this.As<short>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        int IConvertible.ToInt32(IFormatProvider provider)
            => this.As<int>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        long IConvertible.ToInt64(IFormatProvider provider)
            => this.As<long>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        sbyte IConvertible.ToSByte(IFormatProvider provider)
            => this.As<sbyte>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        float IConvertible.ToSingle(IFormatProvider provider)
            => this.As<float>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        string IConvertible.ToString(IFormatProvider provider)
            => this.ToString();

        /// <inheritdoc />
        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            var type = typeof(ValueString);
            var providerType = typeof(IFormatProvider);
            var method = GetMethod(type, "As", new[] { providerType }).MakeGenericMethod(conversionType);
            return method.Invoke(this, new[] { provider ?? CultureInfo.InvariantCulture });
        }

        /// <inheritdoc />
        ushort IConvertible.ToUInt16(IFormatProvider provider)
            => this.As<ushort>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        uint IConvertible.ToUInt32(IFormatProvider provider)
            => this.As<uint>(provider ?? CultureInfo.InvariantCulture);

        /// <inheritdoc />
        ulong IConvertible.ToUInt64(IFormatProvider provider)
            => this.As<ulong>(provider ?? CultureInfo.InvariantCulture);
    }
}

#endif
