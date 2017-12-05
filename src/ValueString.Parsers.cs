// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Dawn
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <content>Contains the parsers.</content>
    public partial struct ValueString
    {
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

            /// <summary>The <see cref="System.Nullable{T}" /> type.</summary>
            private readonly Type nullableValueType;

            /// <summary>The <see cref="Nullable{T}" /> type.</summary>
            private readonly Type nullableParserType;

            /// <summary>
            ///     The signature of parser methods
            ///     that accept a format provider.
            /// </summary>
            private readonly Type[] formattableParserSig;

            /// <summary>
            ///     The signature of parser methods that
            ///     do not accept a format provider.
            /// </summary>
            private readonly Type[] parserSig;

            /// <summary>The signature of number parser methods.</summary>
            private readonly Type[] numericParserSig;

            /// <summary>The signature of date parser methods.</summary>
            private readonly Type[] dateParserSig;

            /// <summary>
            ///     The parameter expressions of parser methods
            ///     that accept a format provider.
            /// </summary>
            private readonly ParameterExpression[] formattableParserParams;

            /// <summary>
            ///     The parameter expressions of parser methods
            ///     that do not accept a format provider.
            /// </summary>
            private readonly ParameterExpression[] parserParams;

            /// <summary>The parameter expressions of number parser methods.</summary>
            private readonly ParameterExpression[] numericParserParams;

            /// <summary>The parameter expressions of date parser methods.</summary>
            private readonly ParameterExpression[] dateParserParams;

            /// <summary>The <see cref="IFormattable" /> type.</summary>
            private readonly Type formattableType;

            #endregion Fields

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="Parser" /> class.
            /// </summary>
            private Parser()
            {
                this.objectType = typeof(object);

                this.stringType = typeof(string);
                this.providerType = typeof(IFormatProvider);
                this.booleanType = typeof(bool);

                this.nullableValueType = typeof(Nullable<>);
                this.nullableParserType = typeof(NullableParser<>);

                this.formattableParserSig = new[] { this.stringType, this.providerType };
                this.parserSig = new[] { this.stringType };

                var nStylesType = typeof(NumberStyles);
                var dStylesType = typeof(DateTimeStyles);
                this.numericParserSig = new[] { this.stringType, nStylesType, this.providerType };
                this.dateParserSig = new[] { this.stringType, this.providerType, dStylesType };

                var stringParam = Expression.Parameter(this.stringType, "s");
                var providerParam = Expression.Parameter(this.providerType, "provider");
                this.formattableParserParams = new[] { stringParam, providerParam };
                this.parserParams = new[] { stringParam };

                var nStylesParam = Expression.Parameter(nStylesType, "styles");
                var dStylesParam = Expression.Parameter(dStylesType, "styles");
                this.numericParserParams = new[] { stringParam, nStylesParam, providerParam };
                this.dateParserParams = new[] { stringParam, providerParam, dStylesParam };

                this.formattableType = typeof(IFormattable);
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
            private PF<T> CompileFormattableParser<T>(MethodInfo method)
            {
                var c = Expression.Call(method, this.formattableParserParams);
                var l = Expression.Lambda<PF<T>>(c, this.formattableParserParams);
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
            private P<T> CompileParser<T>(MethodInfo method)
            {
                var c = Expression.Call(method, this.parserParams);
                var l = Expression.Lambda<P<T>>(c, this.parserParams);
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
            private P<T> CompileCtor<T>(ConstructorInfo constructor)
            {
                var c = Expression.New(constructor, this.parserParams);
                var l = Expression.Lambda<P<T>>(c, this.parserParams);
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
            private TPF<T> CompileSafeFormattableParser<T>(MethodInfo method, Type outTargetType)
            {
                var o = Expression.Parameter(outTargetType, "result");
                var p = this.GetAdded(this.formattableParserParams, o);
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
            private TPF<T> CompileSafeNumericParser<T>(
                MethodInfo method, Type targetType, Type outTargetType)
            {
                var o = Expression.Parameter(outTargetType, "result");
                var p = this.GetAdded(this.numericParserParams, o);
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
            private TPF<T> CompileSafeDateParser<T>(MethodInfo method, Type outTargetType)
            {
                var o = Expression.Parameter(outTargetType, "result");
                var p = this.GetAdded(this.dateParserParams, o);
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
            private TP<T> CompileSafeParser<T>(MethodInfo method, Type outTargetType)
            {
                var o = Expression.Parameter(outTargetType, "result");
                var p = this.GetAdded(this.parserParams, o);
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
#if ALT_REFLECTION
                var i = type.GetTypeInfo();
                var n = !i.IsClass
                    && i.IsGenericType
                    && i.GetGenericTypeDefinition() == this.nullableValueType;

                underlyingType = n ? type.GenericTypeArguments[0] : type;
#else
                var n = !type.IsClass
                    && type.IsGenericType
                    && type.GetGenericTypeDefinition() == this.nullableValueType;

                underlyingType = n ? type.GetGenericArguments()[0] : type;
#endif

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
            private T CompileNullableParser<T>(Type targetType, string fieldName)
            {
                var nullableParserType = this.nullableParserType.MakeGenericType(targetType);
                var f = Expression.Field(null, GetField(nullableParserType, fieldName));
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

            /// <summary>Converts objects to culture-invariant strings.</summary>
            /// <typeparam name="T">The type to convert to string.</typeparam>
            public static class Formatter<T>
            {
                /// <summary>The cached formatter.</summary>
                public static readonly Func<T, IFormatProvider, string> Format = InitFormat();

                /// <summary>
                ///     Initializes a formatter for type <typeparamref name="T" />.
                /// </summary>
                /// <returns>
                ///     A delegate that converts the specified object to string
                ///     using the specified format provider where possible.
                /// </returns>
                private static Func<T, IFormatProvider, string> InitFormat()
                {
                    var sourceType = typeof(T);
#if ALT_REFLECTION
                    var info = sourceType.GetTypeInfo();
                    var isFormattable = common.formattableType.GetTypeInfo().IsAssignableFrom(info);
                    var isValueType = info.IsValueType;
#else
                    var isFormattable = common.formattableType.IsAssignableFrom(sourceType);
                    var isValueType = sourceType.IsValueType;
#endif

                    if (isFormattable)
                    {
                        var instance = Expression.Parameter(sourceType, "this");
                        var method = GetMethod(sourceType, "ToString", common.formattableParserSig);
                        var call = Expression.Call(instance, method, common.formattableParserParams);
                        var lambda = Expression.Lambda<Func<T, string, IFormatProvider, string>>(
                            call, instance, common.formattableParserParams[0], common.formattableParserParams[1]);

                        var compiled = lambda.Compile();
                        if (isValueType)
                            return (f, provider) => compiled(f, null, provider);
                        else
                            return (f, provider) => f != null ? compiled(f, null, provider) : null;
                    }

                    if (isValueType)
                        return (o, provider) => o.ToString();
                    else
                        return (o, provider) => o?.ToString();
                }
            }

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
                public static readonly PF<T> Parse = InitParse(true);

                /// <summary>The cached safe parser.</summary>
                /// <remarks>This field cannot be <c>null</c>.</remarks>
                public static readonly TPF<T> TryParse = InitTryParse(true);

                #endregion Fields

                #region Methods

                /// <summary>
                ///     Initializes a parser of type <typeparamref name="T" />.
                /// </summary>
                /// <param name="fallbackToTryParse">
                ///     Whether to fall back to <see cref="InitTryParse(bool)" />
                ///     if no suitable parser method is found.
                /// </param>
                /// <returns>
                ///     A delegate that parses the specified string using the
                ///     specified format provider to type <typeparamref name="T" />,
                ///     if one could be initialized; otherwise, <c>null</c>.
                /// </returns>
                private static PF<T> InitParse(bool fallbackToTryParse)
                {
                    var targetType = typeof(T);

                    // Return if the target type is directly assignable from string.
                    if (targetType == common.objectType ||
                        targetType == common.stringType)
                        return (s, provider) => (T)(object)s;

                    // Initialize a nullable value parser if the target type is nullable.
                    if (common.IsNullable(targetType, out targetType))
                        return common.CompileNullableParser<PF<T>>(targetType, nameof(Parse));

                    // Search for the parsing method.
                    const string name = "Parse";
                    var method = GetMethod(targetType, name, common.formattableParserSig);
                    if (method?.ReturnType == targetType && method.IsStatic)
                        return InitParse(targetType, common.CompileFormattableParser<T>(method));

                    method = GetMethod(targetType, name, common.parserSig);
                    if (method?.ReturnType == targetType && method.IsStatic)
                    {
                        var f = common.CompileParser<T>(method);
                        return InitParse(targetType, (s, provider) => f(s));
                    }

                    // Wrap the safe parser.
                    if (fallbackToTryParse)
                    {
                        var parser = InitTryParse(false);
                        if (parser != null)
                            return InitParse(targetType, (s, provider) =>
                            {
                                return parser(s, provider, out var result)
                                    ? result
                                    : throw new FormatException();
                            });
                    }

                    // Check for the TypeConverterAttribute.
#if TYPE_DESCRIPTOR
                    var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
                    if (converter.CanConvertFrom(common.stringType))
                        return (s, provider) => (T)converter.ConvertFromString(null, provider as CultureInfo, s);
#endif

                    // Search for a suitable constructor.
                    if (TryGetConstructor(targetType, common.parserSig, out var constructor))
                    {
                        var f = common.CompileCtor<T>(constructor);
                        return InitParse(targetType, (s, provider) => f(s));
                    }

                    // Check for a custom parser.
                    if (CustomParser.TryGetParser(targetType, out TPF<T> custom))
                        return (s, provider) =>
                        {
                            return custom(s, provider, out var result)
                                ? result
                                : throw new FormatException();
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
                private static PF<T> InitParse(Type targetType, PF<T> parser)
                {
                    return !CustomParser.TryGetParser(targetType, out TPF<T> custom)
                        ? parser
                        : (s, provider) =>
                        {
                            return custom(s, provider, out var result)
                                ? result
                                : parser(s, provider);
                        };
                }

                /// <summary>
                ///     Initializes a safe parser of type <typeparamref name="T" />.
                /// </summary>
                /// <param name="fallbackToParse">
                ///     Whether to fall back to <see cref="InitParse(bool)" />
                ///     if no suitable, safe parser method is found.
                /// </param>
                /// <returns>
                ///     A safe delegate that parses the specified string using the
                ///     specified format provider to type <typeparamref name="T" />.
                /// </returns>
                private static TPF<T> InitTryParse(bool fallbackToParse)
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
                        return common.CompileNullableParser<TPF<T>>(targetType, nameof(TryParse));

                    // Search for a parsing method.
                    const string name = "TryParse";
                    var outTargetType = targetType.MakeByRefType();

                    var sig = common.GetAdded(common.formattableParserSig, outTargetType);
                    var method = GetMethod(targetType, name, sig);
                    if (method?.ReturnType == common.booleanType && method.IsStatic)
                        return InitTryParse(
                            targetType,
                            common.CompileSafeFormattableParser<T>(method, outTargetType));

                    sig = common.GetAdded(common.numericParserSig, outTargetType);
                    method = GetMethod(targetType, name, sig);
                    if (method?.ReturnType == common.booleanType && method.IsStatic)
                        return InitTryParse(
                            targetType,
                            common.CompileSafeNumericParser<T>(method, targetType, outTargetType));

                    sig = common.GetAdded(common.dateParserSig, outTargetType);
                    method = GetMethod(targetType, name, sig);
                    if (method?.ReturnType == common.booleanType && method.IsStatic)
                        return InitTryParse(
                            targetType,
                            common.CompileSafeDateParser<T>(method, outTargetType));

                    sig = common.GetAdded(common.parserSig, outTargetType);
                    method = GetMethod(targetType, name, sig);
                    if (method?.ReturnType == common.booleanType && method.IsStatic)
                    {
                        var f = common.CompileSafeParser<T>(method, outTargetType);
                        return InitTryParse(
                            targetType,
                            (string s, IFormatProvider provider, out T result) => f(s, out result));
                    }

                    // Wrap the regular parser.
                    if (!fallbackToParse)
                        return null;

                    var parser = InitParse(false);
                    if (parser != null)
                        return InitTryParse(targetType, (string s, IFormatProvider provider, out T result) =>
                        {
                            try
                            {
                                result = parser(s, provider);
                                return true;
                            }
                            catch (Exception)
                            {
                                result = default;
                                return false;
                            }
                        });

                    // Return a failure parser.
                    return InitTryParse(targetType, (string s, IFormatProvider provider, out T result) =>
                    {
                        result = default;
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
                private static TPF<T> InitTryParse(Type targetType, TPF<T> parser)
                {
                    return !CustomParser.TryGetParser(targetType, out TPF<T> custom)
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
#if ALT_REFLECTION
                    var constructors = type.GetTypeInfo().DeclaredConstructors;
#else
                    var constructors = type.GetConstructors();
#endif
                    foreach (var c in constructors)
                    {
                        var parameters = c.GetParameters();
                        if (parameters.Length != common.parserSig.Length)
                            continue;

                        var mismatch = false;
                        for (var i = 0; i < parameters.Length; i++)
                            if (parameters[i].ParameterType != common.parserSig[i])
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
                public static readonly PF<T?> Parse = InitParse();

                /// <summary>The cached safe parser.</summary>
                /// <remarks>This field cannot be <c>null</c>.</remarks>
                public static readonly TPF<T?> TryParse = InitTryParse();

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
                ///     The underlying parser uses the <see cref="DefaultParser{T}.Parse" />
                ///     when the specified string is not null.
                /// </remarks>
                private static PF<T?> InitParse()
                {
                    var f = DefaultParser<T>.Parse;
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
                ///     The underlying parser uses the <see cref="DefaultParser{T}.TryParse" />
                ///     when the specified string is not null.
                /// </remarks>
                private static TPF<T?> InitTryParse()
                {
                    return (string s, IFormatProvider provider, out T? result) =>
                    {
                        if (s == null)
                        {
                            result = default;
                            return true;
                        }

                        if (DefaultParser<T>.TryParse(s, provider, out var r))
                        {
                            result = r;
                            return true;
                        }

                        result = default;
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
                private static readonly Dictionary<Type, object> parsers
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
                    if (parsers.TryGetValue(type, out var p))
                    {
                        parser = p as TPF<T>;
                        return true;
                    }

                    parser = null;
                    return false;
                }

                /// <summary>Initializes the custom parsers.</summary>
                /// <returns>A collection of custom parsers.</returns>
                private static Dictionary<Type, object> InitParsers()
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

                                result = default;
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
    }
}
