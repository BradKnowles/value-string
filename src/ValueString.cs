// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Dawn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>Represents arbitrary data as string.</summary>
    /// <remarks>
    ///     <see cref="ValueString" /> methods are culture-insensitive by default.
    ///     So the invariant culture (<see cref="CultureInfo.InvariantCulture" />)
    ///     is used unless one of the overloads accepting
    ///     an <see cref="IFormatProvider" /> is used.
    /// </remarks>
    [DataContract]
    [DebuggerDisplay("{data}")]
    public struct ValueString : IEquatable<ValueString>, IEquatable<string>
    {
        #region Fields

        /// <summary>The data.</summary>
        [DataMember]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string data;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueString" /> struct.
        /// </summary>
        /// <param name="data">The data to hold as string.</param>
        /// <remarks>
        ///     Invariant culture will be used converting the data to string
        ///     if its type implements <see cref="IFormattable" />.
        /// </remarks>
        public ValueString(object data)
        {
            var f = data as IFormattable;
            this.data = f != null
                ? f.ToString(null, CultureInfo.InvariantCulture)
                : data?.ToString();
        }

        #endregion Constructors

        #region Operators

        /// <summary>An implicit conversion operator from a string.</summary>
        /// <param name="data">The string data to wrap.</param>
        public static implicit operator ValueString(string data)
            => new ValueString(data);

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
                return Parser.DefaultParser<T>.Func(this.data, provider);
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
            => Parser.DefaultParser<T>.TryFunc(this.data, provider, out result);

        /// <summary>Gets the underlying string data of the value.</summary>
        /// <param name="result">
        ///     When this method returns, contains the
        ///     string data of the current value.
        /// </param>
        /// <returns><c>true</c>.</returns>
        public bool Is(out string result)
        {
            result = this.data;
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
            T @this;
            return this.Is(provider, out @this)
                && EqualityComparer<T>.Default.Equals(@this, other);
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
        {
            return ReferenceEquals(this.data, other.data)
                || (this.data?.Equals(other.data, comparison) ?? false);
        }

        /// <summary>
        ///     Determines whether the specified string
        ///     value is equal to this instance.
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
        public bool Equals(ValueString other) => this.data == other.data;

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
        bool IEquatable<string>.Equals(string other) => this.Equals(other);

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
                return this.data == null;

            var s = obj as string;
            if (s != null)
                return this.Equals(s);

            return obj is ValueString && this.Equals((ValueString)obj);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => this.data?.GetHashCode() ?? 0;

        /// <summary>Returns the underlying string data.</summary>
        /// <returns><see cref="data" />.</returns>
        public override string ToString() => this.data;

        #endregion Methods

        #region Classes

        /// <summary>Provides utilities for parsing strings to arbitrary types.</summary>
        private sealed class Parser
        {
            #region Fields

            /// <summary>The only instance of <see cref="Parser" />.</summary>
            private static readonly Parser common = new Parser();

            /// <summary>The <see cref="object" /> type.</summary>
            private readonly Type objectType;

            /// <summary>The <see cref="string" /> type.</summary>
            private readonly Type stringType;

            /// <summary>The <see cref="IFormatProvider" /> type.</summary>
            private readonly Type providerType;

            /// <summary>The <see cref="bool" /> type.</summary>
            private readonly Type booleanType;

            /// <summary>The <see cref="decimal" /> type.</summary>
            private readonly Type decimalType;

            /// <summary>The <see cref="double" /> type.</summary>
            private readonly Type doubleType;

            /// <summary>The <see cref="float" /> type.</summary>
            private readonly Type floatType;

            /// <summary>The <see cref="System.Nullable{T}" /> type.</summary>
            private readonly Type nullableValueType;

            /// <summary>The <see cref="Nullable{T}" /> type.</summary>
            private readonly Type nullableParserType;

            /// <summary>
            ///     The signature of parser methods
            ///     that accept a format provider.
            /// </summary>
            private readonly Type[] pfSig;

            /// <summary>
            ///     The signature of parser methods that
            ///     do not accept a format provider.
            /// </summary>
            private readonly Type[] pSig;

            /// <summary>
            ///     The signature of number parser methods
            ///     that do not accept a format provider.
            /// </summary>
            private readonly Type[] npfSig;

            /// <summary>
            ///     The signature of date parser methods
            ///     that do not accept a format provider.
            /// </summary>
            private readonly Type[] dpfSig;

            /// <summary>
            ///     The parameter expressions of parser methods
            ///     that accept a format provider.
            /// </summary>
            private readonly ParameterExpression[] pfParams;

            /// <summary>
            ///     The parameter expressions of parser methods
            ///     that do not accept a format provider.
            /// </summary>
            private readonly ParameterExpression[] pParams;

            /// <summary>
            ///     The parameter expressions of number parser
            ///     methods that accept a format provider.
            /// </summary>
            private readonly ParameterExpression[] npfParams;

            /// <summary>
            ///     The parameter expressions of date parser
            ///     methods that accept a format provider.
            /// </summary>
            private readonly ParameterExpression[] dpfParams;

            #endregion Fields

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the
            ///     <see cref="Parser" /> class.
            /// </summary>
            private Parser()
            {
                this.objectType = typeof(object);

                this.stringType = typeof(string);
                this.providerType = typeof(IFormatProvider);
                this.booleanType = typeof(bool);

                this.decimalType = typeof(decimal);
                this.doubleType = typeof(double);
                this.floatType = typeof(float);

                this.nullableValueType = typeof(System.Nullable<>);
                this.nullableParserType = typeof(Nullable<>);

                this.pfSig = new[] { this.stringType, this.providerType };
                this.pSig = new[] { this.stringType };

                var nStylesType = typeof(NumberStyles);
                var dStylesType = typeof(DateTimeStyles);
                this.npfSig = new[] { this.stringType, nStylesType, this.providerType };
                this.dpfSig = new[] { this.stringType, this.providerType, dStylesType };

                var stringParam = Expression.Parameter(this.stringType, "s");
                var providerParam = Expression.Parameter(this.providerType, "provider");
                this.pfParams = new[] { stringParam, providerParam };
                this.pParams = new[] { stringParam };

                var nStylesParam = Expression.Parameter(nStylesType, "styles");
                var dStylesParam = Expression.Parameter(dStylesType, "styles");
                this.npfParams = new[] { stringParam, nStylesParam, providerParam };
                this.dpfParams = new[] { stringParam, providerParam, dStylesParam };
            }

            #endregion Constructors

            #region Delegates

            /// <summary>
            ///     Encapsulates a method that parses a string to an instance
            ///     of type <typeparamref name="T" /> using a specified object
            ///     that supplies culture-specific parsing information.
            /// </summary>
            /// <typeparam name="T">Type to convert the string to.</typeparam>
            /// <param name="s">The string to convert.</param>
            /// <param name="provider">
            ///     An object that supplies culture-specific parsing information.
            /// </param>
            /// <returns>The object converted from the string.</returns>
            public delegate T PF<T>(string s, IFormatProvider provider);

            /// <summary>
            ///     Encapsulates a safe method that parses a string
            ///     to an instance of type <typeparamref name="T" />
            ///     using a specified object that supplies
            ///     culture-specific parsing information.
            /// </summary>
            /// <typeparam name="T">Type to convert the string to.</typeparam>
            /// <param name="s">The string to convert.</param>
            /// <param name="provider">
            ///     An object that supplies culture-specific parsing information.
            /// </param>
            /// <param name="result">
            ///     When the method returns, contains the object converted
            ///     from the string, if the conversion succeeded; otherwise,
            ///     a default <typeparamref name="T" />.
            /// </param>
            /// <returns>
            ///     <c>true</c>, if the conversion succeeded;
            ///     otherwise, <c>false</c>.
            /// </returns>
            public delegate bool TPF<T>(string s, IFormatProvider provider, out T result);

            /// <summary>
            ///     Encapsulates a method that parses a string to
            ///     an instance of type <typeparamref name="T" />.
            /// </summary>
            /// <typeparam name="T">Type to convert the string to.</typeparam>
            /// <param name="s">The string to convert.</param>
            /// <returns>The object converted from the string.</returns>
            private delegate T P<T>(string s);

            /// <summary>
            ///     Encapsulates a safe method that parses a string
            ///     to an instance of type <typeparamref name="T" />.
            /// </summary>
            /// <typeparam name="T">Type to convert the string to.</typeparam>
            /// <param name="s">The string to convert.</param>
            /// <param name="result">
            ///     When the method returns, contains the object converted
            ///     from the string, if the conversion succeeded; otherwise,
            ///     a default <typeparamref name="T" />.
            /// </param>
            /// <returns>
            ///     <c>true</c>, if the conversion succeeded;
            ///     otherwise, <c>false</c>.
            /// </returns>
            private delegate bool TP<T>(string s, out T result);

            /// <summary>
            ///     Encapsulates a safe method that parses a string to an
            ///     instance of the numeric type <typeparamref name="T" />.
            /// </summary>
            /// <typeparam name="T">Type to convert the string to.</typeparam>
            /// <param name="s">The string to convert.</param>
            /// <param name="styles">The styles permitted in <paramref name="s" />.</param>
            /// <param name="provider">
            ///     An object that supplies culture-specific parsing information.
            /// </param>
            /// <param name="result">
            ///     When the method returns, contains the object converted
            ///     from the string, if the conversion succeeded; otherwise,
            ///     a default <typeparamref name="T" />.
            /// </param>
            /// <returns>
            ///     <c>true</c>, if the conversion succeeded;
            ///     otherwise, <c>false</c>.
            /// </returns>
            private delegate bool NTPF<T>(string s, NumberStyles styles, IFormatProvider provider, out T result);

            /// <summary>
            ///     Encapsulates a safe method that parses a string to an
            ///     instance of the date/time type <typeparamref name="T" />.
            /// </summary>
            /// <typeparam name="T">Type to convert the string to.</typeparam>
            /// <param name="s">The string to convert.</param>
            /// <param name="provider">
            ///     An object that supplies culture-specific parsing information.
            /// </param>
            /// <param name="styles">The styles permitted in <paramref name="s" />.</param>
            /// <param name="result">
            ///     When the method returns, contains the object converted
            ///     from the string, if the conversion succeeded; otherwise,
            ///     a default <typeparamref name="T" />.
            /// </param>
            /// <returns>
            ///     <c>true</c>, if the conversion succeeded;
            ///     otherwise, <c>false</c>.
            /// </returns>
            private delegate bool DTPF<T>(string s, IFormatProvider provider, DateTimeStyles styles, out T result);

            #endregion Delegates

            #region Methods

            /// <summary>
            ///     Compiles the specified method as a strongly-typed
            ///     parser that accepts a format provider.
            /// </summary>
            /// <typeparam name="T">The target type of the parser.</typeparam>
            /// <param name="method">The method to compile.</param>
            /// <returns>
            ///     A delegate that parses a string to an
            ///     instance of <typeparamref name="T" />.
            /// </returns>
            private PF<T> GetPF<T>(MethodInfo method)
            {
                var c = Expression.Call(method, this.pfParams);
                var l = Expression.Lambda<PF<T>>(c, this.pfParams);
                return l.Compile();
            }

            /// <summary>
            ///     Compiles the specified method as a strongly-typed
            ///     parser that does not accept a format provider.
            /// </summary>
            /// <typeparam name="T">The target type of the parser.</typeparam>
            /// <param name="method">The method to compile.</param>
            /// <returns>
            ///     A delegate that parses a string to an
            ///     instance of <typeparamref name="T" />.
            /// </returns>
            private P<T> GetP<T>(MethodInfo method)
            {
                var c = Expression.Call(method, this.pParams);
                var l = Expression.Lambda<P<T>>(c, this.pParams);
                return l.Compile();
            }

            /// <summary>
            ///     Compiles the specified constructor
            ///     as a strongly-typed parser.
            /// </summary>
            /// <typeparam name="T">The target type of the parser.</typeparam>
            /// <param name="constructor">The constructor to compile.</param>
            /// <returns>
            ///     A delegate that parses a string to an
            ///     instance of <typeparamref name="T" />.
            /// </returns>
            private P<T> GetP<T>(ConstructorInfo constructor)
            {
                var c = Expression.New(constructor, this.pParams);
                var l = Expression.Lambda<P<T>>(c, this.pParams);
                return l.Compile();
            }

            /// <summary>
            ///     Compiles the specified method as a safe, strongly-typed
            ///     parser that accepts a format provider and returns a
            ///     value indicating whether the conversion succeeded.
            /// </summary>
            /// <typeparam name="T">The target type of the parser.</typeparam>
            /// <param name="method">The method to compile.</param>
            /// <param name="outTargetType">
            ///     The target type as a reference parameter.
            /// </param>
            /// <returns>
            ///     A delegate that parses a string to an
            ///     instance of <typeparamref name="T" />.
            /// </returns>
            private TPF<T> GetTPF<T>(MethodInfo method, Type outTargetType)
            {
                var o = Expression.Parameter(outTargetType, "result");
                var p = this.GetAdded(this.pfParams, o);
                var c = Expression.Call(method, p);
                var l = Expression.Lambda<TPF<T>>(c, p);
                return l.Compile();
            }

            /// <summary>
            ///     Compiles the specified method as a safe, strongly-typed
            ///     numeric parser that accepts a format provider and returns
            ///     a value indicating whether the conversion succeeded.
            /// </summary>
            /// <typeparam name="T">The target type of the parser.</typeparam>
            /// <param name="method">The method to compile.</param>
            /// <param name="targetType">T
            ///     The runtime instance of the parser's target type.
            /// </param>
            /// <param name="outTargetType">
            ///     The target type as a reference parameter.
            /// </param>
            /// <returns>
            ///     A delegate that parses a string to an
            ///     instance of <typeparamref name="T" />.
            /// </returns>
            private TPF<T> GetNumericTPF<T>(MethodInfo method, Type targetType, Type outTargetType)
            {
                var o = Expression.Parameter(outTargetType, "result");
                var p = this.GetAdded(this.npfParams, o);
                var c = Expression.Call(method, p);
                var l = Expression.Lambda<NTPF<T>>(c, p);
                var d = l.Compile();
                return (string s, IFormatProvider provider, out T result) =>
                    d(s, NumberStyles.Number, provider, out result);
            }

            /// <summary>
            ///     Compiles the specified method as a safe, strongly-typed
            ///     date parser that accepts a format provider and returns
            ///     a value indicating whether the conversion succeeded.
            /// </summary>
            /// <typeparam name="T">The target type of the parser.</typeparam>
            /// <param name="method">The method to compile.</param>
            /// <param name="outTargetType">
            ///     The target type as a reference parameter.
            /// </param>
            /// <returns>
            ///     A delegate that parses a string to an
            ///     instance of <typeparamref name="T" />.
            /// </returns>
            private TPF<T> GetDateTPF<T>(MethodInfo method, Type outTargetType)
            {
                var o = Expression.Parameter(outTargetType, "result");
                var p = this.GetAdded(this.dpfParams, o);
                var c = Expression.Call(method, p);
                var l = Expression.Lambda<DTPF<T>>(c, p);
                var d = l.Compile();
                return (string s, IFormatProvider provider, out T result) =>
                    d(s, provider, DateTimeStyles.None, out result);
            }

            /// <summary>
            ///     Compiles the specified method as a safe, strongly-typed
            ///     parser that does not accept a format provider and returns
            ///     a value indicating whether the conversion succeeded.
            /// </summary>
            /// <typeparam name="T">The target type of the parser.</typeparam>
            /// <param name="method">The method to compile.</param>
            /// <param name="outTargetType">
            ///     The target type as a reference parameter.
            /// </param>
            /// <returns>
            ///     A delegate that parses a string to an
            ///     instance of <typeparamref name="T" />.
            /// </returns>
            private TP<T> GetTP<T>(MethodInfo method, Type outTargetType)
            {
                var o = Expression.Parameter(outTargetType, "result");
                var p = this.GetAdded(this.pParams, o);
                var c = Expression.Call(method, p);
                var lambda = Expression.Lambda<TP<T>>(c, p);
                return lambda.Compile();
            }

            /// <summary>
            ///     Returns a value indicating whether the
            ///     specified type is a nullable struct.
            /// </summary>
            /// <param name="type">The type to check.</param>
            /// <param name="underlyingType">
            ///     When this method returns, contains the first generic
            ///     argument of <paramref name="type" /> if it is a nullable
            ///     struct; otherwise, the <paramref name="type" /> itself.
            /// </param>
            /// <returns>
            ///     <c>true</c>, if <paramref name="type" /> is
            ///     a nullable struct; otherwise, <c>false</c>.
            /// </returns>
            private bool IsNullable(Type type, out Type underlyingType)
            {
                var i = type.GetTypeInfo();
                var n = !i.IsClass
                    && i.IsGenericType
                    && i.GetGenericTypeDefinition() == this.nullableValueType;

                underlyingType = n ? type.GenericTypeArguments[0] : type;
                return n;
            }

            /// <summary>Gets the parser for the specified nullable type.</summary>
            /// <typeparam name="T">The type of the parser.</typeparam>
            /// <param name="targetType">The target type of the parser.</param>
            /// <param name="fieldName">
            ///     The name of the parser delegate in <see cref="Nullable{T}" />
            ///     class to call as underlying parser.
            /// </param>
            /// <returns>
            ///     A strongly-typed parser for the specified target type.
            /// </returns>
            private T GetNullablePF<T>(Type targetType, string fieldName)
            {
                var nullableParserType = this.nullableParserType.MakeGenericType(targetType);
                var f = Expression.Field(null, nullableParserType.GetRuntimeField(fieldName));
                var l = Expression.Lambda<Func<T>>(f, null);
                return l.Compile()();
            }

            /// <summary>
            ///     Returns a new array that contains the
            ///     items of the specified array with the
            ///     specified item added to the end.
            /// </summary>
            /// <typeparam name="T">The type of the array items.</typeparam>
            /// <param name="src">The source array.</param>
            /// <param name="item">The item to add.</param>
            /// <returns>
            ///     A new array containing <paramref name="src" />'s original
            ///     items with <paramref name="item" /> added to the end.
            /// </returns>
            private T[] GetAdded<T>(T[] src, T item)
            {
                var result = new T[src.Length + 1];
                src.CopyTo(result, 0);
                result[src.Length] = item;

                return result;
            }

            #endregion Methods

            #region Classes

            /// <summary>
            ///     Provides a cached parser for parsing strings
            ///     to instances of <typeparamref name="T" />.
            /// </summary>
            /// <typeparam name="T">The type to convert from string.</typeparam>
            public static class DefaultParser<T>
            {
                #region Fields

                /// <summary>The cached parser.</summary>
                /// <remarks>
                ///     This field can be <c>null</c>.
                ///     This indicates no delegate could be initialized that
                ///     parses a string to the type <typeparamref name="T" />.
                /// </remarks>
                public static readonly PF<T> Func = InitFunc(true);

                /// <summary>The cached safe parser.</summary>
                /// <remarks>This field cannot be <c>null</c>.</remarks>
                public static readonly TPF<T> TryFunc = InitTryFunc(true);

                #endregion Fields

                #region Methods

                /// <summary>
                ///     Initializes a parser of type <typeparamref name="T" />.
                /// </summary>
                /// <param name="fallbackToTryParse">
                ///     Whether to fall back to <see cref="InitTryFunc(bool)" />
                ///     if no suitable parser method is found.
                /// </param>
                /// <returns>
                ///     A delegate that parses the specified string using the
                ///     specified format provider to type <typeparamref name="T" />,
                ///     if one could be initialized; otherwise, <c>null</c>.
                /// </returns>
                private static PF<T> InitFunc(bool fallbackToTryParse)
                {
                    var targetType = typeof(T);

                    // Return if the target type is directly assignable from string.
                    if (targetType == common.objectType ||
                        targetType == common.stringType)
                        return (s, provider) => (T)(object)s;

                    // Initialize a nullable value parser if the target type is nullable.
                    if (common.IsNullable(targetType, out targetType))
                        return common.GetNullablePF<PF<T>>(targetType, nameof(Func));

                    // Search for the parsing method.
                    const string name = "Parse";
                    var method = targetType.GetRuntimeMethod(name, common.pfSig);
                    if (method?.ReturnType == targetType && method.IsStatic)
                        return InitFunc(targetType, common.GetPF<T>(method));

                    method = targetType.GetRuntimeMethod(name, common.pSig);
                    if (method?.ReturnType == targetType && method.IsStatic)
                    {
                        var f = common.GetP<T>(method);
                        return InitFunc(targetType, (s, provider) => f(s));
                    }

                    // Wrap the safe parser.
                    if (fallbackToTryParse)
                    {
                        var parser = InitTryFunc(false);
                        if (parser != null)
                            return InitFunc(targetType, (s, provider) =>
                            {
                                T result;
                                if (parser(s, provider, out result))
                                    return result;

                                throw new FormatException();
                            });
                    }

                    // Search for a suitable constructor.
                    ConstructorInfo constructor;
                    if (TryGetConstructor(targetType, common.pSig, out constructor))
                    {
                        var f = common.GetP<T>(constructor);
                        return InitFunc(targetType, (s, provider) => f(s));
                    }

                    // Check for a custom parser.
                    TPF<T> custom;
                    if (CustomParser.TryGetParser(targetType, out custom))
                        return (s, provider) =>
                        {
                            T result;
                            if (custom(s, provider, out result))
                                return result;

                            throw new FormatException();
                        };

                    return null;
                }

                /// <summary>
                ///     Returns a delegate that wraps the specified parser
                ///     in order to support custom defined parsers.
                /// </summary>
                /// <param name="targetType">The target type of the parser.</param>
                /// <param name="parser">The original parser to wrap.</param>
                /// <returns>
                ///     A parser delegate that supports custom parsers and falls
                ///     back to calling the specified <paramref name="parser" />.
                /// </returns>
                private static PF<T> InitFunc(Type targetType, PF<T> parser)
                {
                    TPF<T> custom;
                    return !CustomParser.TryGetParser(targetType, out custom)
                        ? parser
                        : (s, provider) =>
                        {
                            T result;
                            return custom(s, provider, out result)
                                ? result
                                : parser(s, provider);
                        };
                }

                /// <summary>
                ///     Initializes a safe parser of type <typeparamref name="T" />.
                /// </summary>
                /// <param name="fallbackToParse">
                ///     Whether to fall back to <see cref="InitFunc(bool)" />
                ///     if no suitable, safe parser method is found.
                /// </param>
                /// <returns>
                ///     A safe delegate that parses the specified string using the
                ///     specified format provider to type <typeparamref name="T" />.
                /// </returns>
                private static TPF<T> InitTryFunc(bool fallbackToParse)
                {
                    // Return if the target type is directly assignable from string.
                    var targetType = typeof(T);
                    if (targetType == common.objectType ||
                        targetType == common.stringType)
                        return (string s, IFormatProvider provider, out T result) =>
                        {
                            result = (T)(object)s;
                            return true;
                        };

                    // Initialize a nullable value parser if the target type is nullable.
                    if (common.IsNullable(targetType, out targetType))
                        return common.GetNullablePF<TPF<T>>(targetType, nameof(TryFunc));

                    // Search for a parsing method.
                    const string name = "TryParse";
                    var outTargetType = targetType.MakeByRefType();

                    var sig = common.GetAdded(common.pfSig, outTargetType);
                    var method = targetType.GetRuntimeMethod(name, sig);
                    if (method?.ReturnType == common.booleanType && method.IsStatic)
                        return InitTryFunc(
                            targetType,
                            common.GetTPF<T>(method, outTargetType));

                    sig = common.GetAdded(common.npfSig, outTargetType);
                    method = targetType.GetRuntimeMethod(name, sig);
                    if (method?.ReturnType == common.booleanType && method.IsStatic)
                        return InitTryFunc(
                            targetType,
                            common.GetNumericTPF<T>(method, targetType, outTargetType));

                    sig = common.GetAdded(common.dpfSig, outTargetType);
                    method = targetType.GetRuntimeMethod(name, sig);
                    if (method?.ReturnType == common.booleanType && method.IsStatic)
                        return InitTryFunc(
                            targetType,
                            common.GetDateTPF<T>(method, outTargetType));

                    sig = common.GetAdded(common.pSig, outTargetType);
                    method = targetType.GetRuntimeMethod(name, sig);
                    if (method?.ReturnType == common.booleanType && method.IsStatic)
                    {
                        var f = common.GetTP<T>(method, outTargetType);
                        return InitTryFunc(
                            targetType,
                            (string s, IFormatProvider provider, out T result) => f(s, out result));
                    }

                    // Wrap the regular parser.
                    if (!fallbackToParse)
                        return null;

                    var parser = InitFunc(false);
                    if (parser != null)
                        return InitTryFunc(targetType, (string s, IFormatProvider provider, out T result) =>
                        {
                            try
                            {
                                result = parser(s, provider);
                                return true;
                            }
                            catch (Exception)
                            {
                                result = default(T);
                                return false;
                            }
                        });

                    // Return a failure parser.
                    return InitTryFunc(targetType, (string s, IFormatProvider provider, out T result) =>
                    {
                        result = default(T);
                        return false;
                    });
                }

                /// <summary>
                ///     Returns a delegate that wraps the specified, safe
                ///     parser in order to support custom defined parsers.
                /// </summary>
                /// <param name="targetType">The target type of the parser.</param>
                /// <param name="parser">The original parser to wrap.</param>
                /// <returns>
                ///     A safe parser delegate that supports custom parsers and falls
                ///     back to calling the specified <paramref name="parser" />.
                /// </returns>
                private static TPF<T> InitTryFunc(Type targetType, TPF<T> parser)
                {
                    TPF<T> custom;
                    return !CustomParser.TryGetParser(targetType, out custom)
                        ? parser
                        : (string s, IFormatProvider provider, out T result) =>
                        {
                            return custom(s, provider, out result)
                                || parser(s, provider, out result);
                        };
                }

                /// <summary>
                ///     Searches for a constructor with the specified
                ///     signature declared by the specified type.
                /// </summary>
                /// <param name="type">The type that declared the constructor.</param>
                /// <param name="sig">The constructor signature.</param>
                /// <param name="constructor">
                ///     When this method returns, contains the constructor
                ///     that is found, if a constructor with the specified
                ///     signature is found; otherwise, <c>null</c>.
                /// </param>
                /// <returns>
                ///     <c>true</c>, if a constructor with the specified
                ///     signature is found; otherwise, <c>false</c>.
                /// </returns>
                private static bool TryGetConstructor(
                    Type type, Type[] sig, out ConstructorInfo constructor)
                {
                    foreach (var c in type.GetTypeInfo().DeclaredConstructors)
                    {
                        var parameters = c.GetParameters();
                        if (parameters.Length != common.pSig.Length)
                            continue;

                        var mismatch = false;
                        for (int i = 0; i < parameters.Length; i++)
                            if (parameters[i].ParameterType != common.pSig[i])
                            {
                                mismatch = true;
                                break;
                            }

                        if (!mismatch)
                        {
                            constructor = c;
                            return true;
                        }
                    }

                    constructor = null;
                    return false;
                }

                #endregion Methods
            }

            /// <summary>
            ///     Provides a cached parser for parsing strings to
            ///     nullable instances of <typeparamref name="T" />.
            /// </summary>
            /// <typeparam name="T">The type to convert from string.</typeparam>
            private static class NullableParser<T>
                where T : struct
            {
                #region Fields

                /// <summary>The cached parser.</summary>
                /// <remarks>
                ///     This field can be <c>null</c>.
                ///     This indicates no delegate could be initialized that
                ///     parses a string to the type <typeparamref name="T" />.
                /// </remarks>
                public static readonly PF<T?> Func = InitFunc();

                /// <summary>The cached safe parser.</summary>
                /// <remarks>This field cannot be <c>null</c>.</remarks>
                public static readonly TPF<T?> TryFunc = InitTryFunc();

                #endregion Fields

                #region Methods

                /// <summary>
                ///     Initializes a parser of type <typeparamref name="T" />.
                /// </summary>
                /// <returns>
                ///     A delegate that parses the specified string using the
                ///     specified format provider to type <typeparamref name="T" />,
                ///     if one could be initialized; otherwise, <c>null</c>.
                /// </returns>
                /// <remarks>
                ///     The underlying parser uses the <see cref="DefaultParser{T}.Func" />
                ///     when the specified string is not null.
                /// </remarks>
                private static PF<T?> InitFunc()
                {
                    var f = DefaultParser<T>.Func;
                    if (f != null)
                        return (s, provider) => s != null ? f(s, provider) : default(T?);

                    return null;
                }

                /// <summary>
                ///     Initializes a safe parser of type <typeparamref name="T" />.
                /// </summary>
                /// <returns>
                ///     A delegate that parses the specified string using the
                ///     specified format provider to type <typeparamref name="T" />.
                /// </returns>
                /// <remarks>
                ///     The underlying parser uses the <see cref="DefaultParser{T}.TryFunc" />
                ///     when the specified string is not null.
                /// </remarks>
                private static TPF<T?> InitTryFunc()
                {
                    return (string s, IFormatProvider provider, out T? result) =>
                    {
                        if (s == null)
                        {
                            result = default(T?);
                            return true;
                        }

                        T r;
                        if (DefaultParser<T>.TryFunc(s, provider, out r))
                        {
                            result = r;
                            return true;
                        }

                        result = default(T?);
                        return false;
                    };
                }

                #endregion Methods
            }

            /// <summary>Provides custom parsers for common types.</summary>
            private static class CustomParser
            {
                #region Fields

                /// <summary>The custom parsers.</summary>
                private static readonly IReadOnlyDictionary<Type, object> parsers
                    = InitParsers();

                #endregion Fields

                #region Methods

                /// <summary>
                ///     Gets the parser that targets the specified type.
                /// </summary>
                /// <typeparam name="T">The target type of the parser.</typeparam>
                /// <param name="type">
                ///     The runtime instance of the parser's target type.
                /// </param>
                /// <param name="parser">
                ///     When this method returns, contains the parser
                ///     for the type <typeparamref name="T" />, if a
                ///     parser is found; otherwise, <c>null</c>.
                /// </param>
                /// <returns>
                ///     <c>true</c>, if a parser for type <typeparamref name="T" />
                ///     is found; otherwise, <c>false</c>.
                /// </returns>
                public static bool TryGetParser<T>(Type type, out TPF<T> parser)
                {
                    object p;
                    if (parsers.TryGetValue(type, out p))
                    {
                        parser = p as TPF<T>;
                        return true;
                    }

                    parser = null;
                    return false;
                }

                /// <summary>Initializes the custom parsers.</summary>
                /// <returns>A collection of custom parsers.</returns>
                private static IReadOnlyDictionary<Type, object> InitParsers()
                {
                    return new Dictionary<Type, object>
                    {
                        [typeof(bool)] = new TPF<bool>(
                            (string s, IFormatProvider provider, out bool result) =>
                            {
                                if (s != null)
                                {
                                    var c = StringComparison.OrdinalIgnoreCase;
                                    if (s.Equals("TRUE", c) || s.Equals("YES", c) || s.Equals("1", c))
                                    {
                                        result = true;
                                        return true;
                                    }

                                    if (s.Equals("FALSE", c) || s.Equals("NO", c) || s.Equals("0", c))
                                    {
                                        result = false;
                                        return true;
                                    }
                                }

                                result = false; // i.e. default(bool)
                                return false;
                            }),
                        [typeof(Uri)] = new TPF<Uri>(
                            (string s, IFormatProvider provider, out Uri result) =>
                                Uri.TryCreate(s, UriKind.Absolute, out result))
                    };
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}
