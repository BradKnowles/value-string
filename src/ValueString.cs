// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Dawn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>Represents data serialized as a culture-neutral (invariant) string.</summary>
    [DebuggerDisplay("{" + nameof(value) + "}")]
    public partial struct ValueString : IEquatable<ValueString>, IEquatable<string>, IXmlSerializable
    {
        #region Fields

        /// <summary>The serialized data.</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string value; // Do not rename. ISerializable implementation uses the name of this field.

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueString" /> struct.
        /// </summary>
        /// <param name="value">The string value.</param>
        private ValueString(string value) => this.value = value;

        #endregion Constructors

        #region Operators

        /// <summary>An implicit conversion operator from a string.</summary>
        /// <param name="value">The string value to wrap.</param>
        public static implicit operator ValueString(string value) => Of(value);

        /// <summary>An equality operator for two value strings.</summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="left" /> and <paramref name="right" />
        ///     are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(ValueString left, ValueString right)
            => left.Equals(right);

        /// <summary>An unequality operator for two value strings.</summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="left" /> and <paramref name="right" />
        ///     are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(ValueString left, ValueString right)
            => !(left == right);

        #endregion Operators

        #region Methods

        /// <summary>
        ///     Initializes a new <see cref="ValueString" /> by converting the
        ///     specified, formattable object using the invariant culture.
        /// </summary>
        /// <typeparam name="T">Type of the value to serialize as string.</typeparam>
        /// <param name="value">The value to serialize as string.</param>
        /// <returns>
        ///     A new <see cref="ValueString" /> containing
        ///     the serialized <paramref name="value" />.
        /// </returns>
        public static ValueString Of<T>(T value)
            => Of(Parser.Formatter<T>.Format(value));

        /// <summary>
        ///     Initializes a new <see cref="ValueString" />
        ///     using the specified string.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>
        ///     A new <see cref="ValueString" /> encapsulating
        ///     <paramref name="value" />.
        /// </returns>
        public static ValueString Of(string value)
            => new ValueString(value);

        /// <summary>
        ///     Converts the value to the given type using the invariant
        ///     culture and returns the converted object.
        /// </summary>
        /// <typeparam name="T">Type to convert the value to.</typeparam>
        /// <returns>The converted value.</returns>
        /// <exception cref="InvalidCastException">
        ///     String cannot be converted to the
        ///     type of <typeparamref name="T" />.
        /// </exception>
        public T As<T>() => this.As<T>(CultureInfo.InvariantCulture);

        /// <summary>
        ///     Converts the value to the given type using the specified
        ///     format provider and returns the converted object.
        /// </summary>
        /// <typeparam name="T">Type to convert the value to.</typeparam>
        /// <param name="provider">
        ///     An object that supplies culture-specific parsing information.
        /// </param>
        /// <returns>The converted value.</returns>
        /// <exception cref="InvalidCastException">
        ///     String cannot be converted to the
        ///     type of <typeparamref name="T" />.
        /// </exception>
        public T As<T>(IFormatProvider provider)
        {
            try
            {
                return Parser.DefaultParser<T>.Parse(this.value, provider);
            }
            catch (Exception x)
            when (!(x is InvalidCastException))
            {
                var m = $"String cannot be converted to {typeof(T).FullName}.";
                throw x is NullReferenceException
                    ? new InvalidCastException(m)
                    : new InvalidCastException(m + " See the inner exception for details.", x);
            }
        }

        /// <summary>
        ///     Converts the value to the given type using the invariant culture.
        ///     A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <typeparam name="T">Type to convert the value to.</typeparam>
        /// <param name="result">
        ///     When this method returns, contains the converted
        ///     value if the conversion succeeded; otherwise,
        ///     a default <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the conversion succeeded;
        ///     otherwise, <c>false</c>.
        /// </returns>
        public bool Is<T>(out T result)
            => this.Is(CultureInfo.InvariantCulture, out result);

        /// <summary>
        ///     Converts the value to the given type
        ///     using the specified format provider.
        ///     A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <typeparam name="T">Type to convert the value to.</typeparam>
        /// <param name="provider">
        ///     An object that supplies culture-specific parsing information.
        /// </param>
        /// <param name="result">
        ///     When this method returns, contains the converted
        ///     value if the conversion succeeded; otherwise,
        ///     a default <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the conversion succeeded;
        ///     otherwise, <c>false</c>.
        /// </returns>
        public bool Is<T>(IFormatProvider provider, out T result)
            => Parser.DefaultParser<T>.TryParse(this.value, provider, out result);

        /// <summary>Gets the underlying string data of the value.</summary>
        /// <param name="result">
        ///     When this method returns, contains the
        ///     string data of the current value.
        /// </param>
        /// <returns><c>true</c>.</returns>
        public bool Is(out string result)
        {
            result = this.value;
            return true;
        }

        /// <summary>
        ///     Converts the value to the given object's type using
        ///     the invariant culture and returns whether
        ///     the converted value is equal to the object.
        /// </summary>
        /// <typeparam name="T">The type of the object to compare.</typeparam>
        /// <param name="other">The object to compare to this value.</param>
        /// <returns>
        ///     <c>true</c>, if the conversion succeeded and the converted value
        ///     is equal to <paramref name="other" />; otherwise, <c>false</c>.
        /// </returns>
        public bool Is<T>(T other) => this.Is(other, CultureInfo.InvariantCulture);

        /// <summary>
        ///     Converts the value to the given object's type using
        ///     the specified format provider and returns whether
        ///     the converted value is equal to the object.
        /// </summary>
        /// <typeparam name="T">The type of the object to compare.</typeparam>
        /// <param name="other">The object to compare to this value.</param>
        /// <param name="provider">
        ///     An object that supplies culture-specific parsing information.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the conversion succeeded and the converted value
        ///     is equal to <paramref name="other" />; otherwise, <c>false</c>.
        /// </returns>
        public bool Is<T>(T other, IFormatProvider provider)
        {
            return this.Is(provider, out T result)
                && EqualityComparer<T>.Default.Equals(result, other);
        }

        /// <summary>
        ///     Finds the specified keys, replace them with their
        ///     respective values and converts the output to the
        ///     given type using the invariant culture.
        /// </summary>
        /// <typeparam name="T">The type to convert the formatted string to.</typeparam>
        /// <param name="values">The key/value pairs to find and replace.</param>
        /// <returns>The formatted and converted <see cref="value" />.</returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="values" /> contains an item with a
        ///     <c>null</c> key or multiple items with the same key.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     The formatted <see cref="value" /> cannot be converted
        ///     to the type of <typeparamref name="T" />.
        /// </exception>
        public T Format<T>(
#if NETSTANDARD2_0
            params (string Key, string Value)[] values
#else
            params KeyValuePair<string, string>[] values
#endif
            )
            => Of(this.Format(values)).As<T>();

        /// <summary>
        ///     Finds the specified keys and replace
        ///     them with their respective values.
        /// </summary>
        /// <param name="values">The key/value pairs to find and replace.</param>
        /// <returns>The formatted <see cref="value" />.</returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="values" /> contains an item with a
        ///     <c>null</c> key or multiple items with the same key.
        /// </exception>
        public string Format(
#if NETSTANDARD2_0
            params (string Key, string Value)[] values
#else
            params KeyValuePair<string, string>[] values
#endif
            )
        {
            if (values == null || values.Length == 0)
                return this.value;

            var replacements = new Dictionary<string, string>(values.Length);
            for (var i = 0; i < values.Length; i++)
            {
                var pair = values[i];
                if (pair.Key == null)
                {
                    var m = "Values cannot contain pairs with null keys.";
                    throw new ArgumentException(m, nameof(values));
                }

                try
                {
                    replacements.Add($"{{{pair.Key}}}", pair.Value ?? string.Empty);
                }
                catch (ArgumentException x)
                {
                    var m = "Values cannot contain pairs with duplicate keys.";
                    throw new ArgumentException(m, nameof(values), x);
                }
            }

            var keys = replacements.Keys.Select(k => Regex.Escape(k));
#if !NET35
            var pattern = string.Join("|", keys);
#else
            var pattern = string.Join("|", keys.ToArray());
#endif
            return new Regex($"({pattern})").Replace(this.value, m => replacements[m.Value]);
        }

        /// <summary>
        ///     Determines whether the specified value string is equal
        ///     to this instance. A parameter specifies the culture,
        ///     case, and sort rules used in the comparison.
        /// </summary>
        /// <param name="other">The value to compare to this instance.</param>
        /// <param name="comparison">
        ///     One of the enumeration values that specifies
        ///     how the value strings will be compared.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the current value is equal to
        ///     <paramref name="other" />; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ValueString other, StringComparison comparison)
            => this.Equals(other.value, comparison);

        /// <summary>
        ///     Determines whether the specified value
        ///     string is equal to this instance.
        /// </summary>
        /// <param name="other">The value to compare to this instance.</param>
        /// <returns>
        ///     <c>true</c>, if the current value is equal to
        ///     <paramref name="other" />; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     This method performs an ordinal (case-sensitive
        ///     and culture-insensitive) comparison.
        /// </remarks>
        public bool Equals(ValueString other) => this.Equals(other.value);

        /// <summary>
        ///     Determines whether the specified string is equal to
        ///     this instance. A parameter specifies the culture,
        ///     case, and sort rules used in the comparison.
        /// </summary>
        /// <param name="other">The value to compare to this instance.</param>
        /// <param name="comparison">
        ///     One of the enumeration values that specifies
        ///     how the value strings will be compared.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the current value is equal to
        ///     <paramref name="other" />; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(string other, StringComparison comparison)
            => ReferenceEquals(this.value, other) || (this.value?.Equals(other, comparison) ?? false);

        /// <summary>
        ///     Determines whether the specified string is equal to this instance.
        /// </summary>
        /// <param name="other">The value to compare to this instance.</param>
        /// <returns>
        ///     <c>true</c>, if the current value is equal to
        ///     <paramref name="other" />; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     This method performs an ordinal (case-sensitive
        ///     and culture-insensitive) comparison.
        /// </remarks>
        public bool Equals(string other) => this.value == other;

        /// <summary>
        ///     Determines whether the object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>
        ///     <c>true</c>, if the current value is equal to
        ///     <paramref name="obj" />; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     This method performs an ordinal (case-sensitive
        ///     and culture-insensitive) comparison for
        ///     <see cref="string" /> and <see cref="ValueString" /> instances.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return this.value == null;

            if (obj is string s)
                return this.Equals(s);

            return obj is ValueString && this.Equals((ValueString)obj);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => this.value?.GetHashCode() ?? 0;

        /// <summary>Returns the underlying string data.</summary>
        /// <returns><see cref="value" />.</returns>
        public override string ToString() => this.value;

        /// <inheritdoc />
        XmlSchema IXmlSerializable.GetSchema() => null;

        /// <inheritdoc />
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.Read())
                this = Of(reader.Value);
        }

        /// <inheritdoc />
        void IXmlSerializable.WriteXml(XmlWriter writer)
            => writer.WriteString(this.value);

        /// <summary>Gets a type's field with the specified name.</summary>
        /// <param name="type">The type containing the field.</param>
        /// <param name="fieldName">The name of the field to find.</param>
        /// <returns>The field info with the specified name.</returns>
        private static FieldInfo GetField(Type type, string fieldName)
        {
#if ALT_REFLECTION
            return type.GetRuntimeField(fieldName);
#else
            return type.GetField(fieldName);
#endif
        }

        /// <summary>Gets a type's method with the specified name and signature.</summary>
        /// <param name="type">The type containing the method.</param>
        /// <param name="methodName">The name of the method to find.</param>
        /// <param name="parameters">The types of the method arguments.</param>
        /// <returns>The method info with the specified name and signature.</returns>
        private static MethodInfo GetMethod(Type type, string methodName, Type[] parameters)
        {
#if ALT_REFLECTION
            return type.GetRuntimeMethod(methodName, parameters);
#else
            return type.GetMethod(methodName, parameters);
#endif
        }

        /// <summary>
        ///     Gets a <see cref="MethodInfo" /> instance of <see cref="As{T}()" />
        ///     or <see cref="As{T}(IFormatProvider)" /> for the specified type.
        /// </summary>
        /// <param name="type">The conversion target.</param>
        /// <param name="withFormatProvider">
        ///     Whether to get the overload accepting a format provider.
        /// </param>
        /// <returns>
        ///     A <see cref="MethodInfo" /> instance of <see cref="As{T}()" />
        ///     or <see cref="As{T}(IFormatProvider)" />.
        /// </returns>
        private static MethodInfo GetAsMethod(Type type, bool withFormatProvider)
        {
            var parameters = withFormatProvider ? new[] { typeof(IFormatProvider) } : new Type[0];
            return GetMethod(typeof(ValueString), "As", parameters).MakeGenericMethod(type);
        }

        #endregion Methods
    }
}
