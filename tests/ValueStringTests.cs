// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Dawn.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using Xunit;

    /// <summary>
    ///     Provides unit tests for the <see cref="ValueString" /> struct
    ///     and related utility extensions (<see cref="ValueStringUtils" />.
    /// </summary>
    public sealed class ValueStringTests
    {
        #region Fields

        /// <summary>The culture info of the United States.</summary>
        private static readonly CultureInfo usCulture = new CultureInfo("en-US");

        /// <summary>The culture info of France.</summary>
        private static readonly CultureInfo frCulture = new CultureInfo("fr-FR");

        #endregion Fields

        #region Methods

        /// <summary>
        ///     Tests whether the <see cref="ValueString" /> constructor uses
        ///     the invariant culture when converting its argument to string.
        /// </summary>
        [Fact(DisplayName = "ValueString ctor uses invariant culture.")]
        public void ValueStringConstructorUsesInvariantCulture()
        {
            CultureInfo.CurrentCulture = frCulture;

            var d = 1.5;
            Assert.Equal("1,5", d.ToString());

            var v = new ValueString(d); // Converted using the invariant culture.
            Assert.Equal("1.5", v.ToString());
        }

        /// <summary>
        ///     Tests whether the <see cref="ValueString" /> methods use the
        ///     invariant culture by default when converting the string data.
        /// </summary>
        [Fact(DisplayName = "ValueString methods use invariant culture.")]
        public void ValueStringMethodsUseInvariantCultureByDefault()
        {
            var time = new DateTime(2016, 11, 28, 22, 59, 58);

            var fr = new ValueString(time.ToString(frCulture));
            Assert.Equal("28/11/2016 22:59:58", fr.ToString());
            Assert.Throws<InvalidCastException>(() => fr.As<DateTime>());
            Assert.Equal(time, fr.As<DateTime>(frCulture));
            Assert.False(fr.Is(out DateTime parsed));
            Assert.True(fr.Is(frCulture, out parsed));
            Assert.False(fr.Is(time));
            Assert.True(fr.Is(time, frCulture));
            Assert.Equal(time, parsed);

            var invariant = new ValueString(time);
            Assert.Equal("11/28/2016 22:59:58", invariant.ToString());
            Assert.Throws<InvalidCastException>(() => invariant.As<DateTime>(frCulture));
            Assert.Equal(time, invariant.As<DateTime>());
            Assert.False(invariant.Is(frCulture, out parsed));
            Assert.True(invariant.Is(out parsed));
            Assert.False(invariant.Is(time, frCulture));
            Assert.True(invariant.Is(time));
            Assert.Equal(time, parsed);
        }

        /// <summary>
        ///     Tests whether the <see cref="ValueString" /> is equatable.
        /// </summary>
        [Fact(DisplayName = "ValueString is equatable.")]
        public void ValueStringIsEquatable()
        {
            var s1 = "a";
            var v1a = new ValueString(s1);
            var v1b = new ValueString(s1);

            var s2 = "A";
            var v2 = new ValueString(s2);

            // As<T>, ToString, Is and Is<T> returns the original string data.
            Assert.NotSame(v1a, v1b);
            Assert.Same(v1a.As<string>(), v1b.ToString());
            Assert.True(v1a.Is(out var temp));
            Assert.Same(s1, temp);
            Assert.True(v1a.Is<string>(out temp));
            Assert.Same(s1, temp);

            // Implements IEquatable<ValueString>.
            var ev = Assert.IsAssignableFrom<IEquatable<ValueString>>(v1a);

            Assert.True(ev.Equals(v1b));
            Assert.False(ev.Equals(v2));

            Assert.True(ev.Equals(s1)); // Implicit conversion from string.
            Assert.False(ev.Equals(s2));

            Assert.True((v1a as object).Equals(v1b)); // Equals(object) is overridden.
            Assert.False((v2 as object).Equals(v1b));

            // Implements IEquatable<string>.
            var es = Assert.IsAssignableFrom<IEquatable<string>>(v1a);

            Assert.True(es.Equals(s1));
            Assert.False(es.Equals(s2));

            Assert.True((v1a as object).Equals(s1)); // Overridden Equals(object) handles strings.
            Assert.False((v1a as object).Equals(s2));

            // Comparison operators.
            Assert.True(v1a == v1b);
            Assert.True(v1a != v2);

            Assert.False(v1a == v2);
            Assert.False(v1a != v1b);

            Assert.True(v1a == s1); // Implicit conversion from string.
            Assert.True(v1a != s2);
            Assert.True(v1a != null);

            // StringComparison overload.
            Assert.False(v1a.Equals(v2, StringComparison.Ordinal));
            Assert.True(v1a.Equals(v2, StringComparison.OrdinalIgnoreCase));

            // Convert and compare.
            Assert.True(v1a.Is(s1));
            Assert.False(v1a.Is(s2));

            v1a = new ValueString(1);
            Assert.True(v1a.Is(1));
            Assert.False(v1a.Is(2));
        }

        /// <summary>
        ///     Tests whether <see cref="ValueString" /> allows <c>null</c> values.
        /// </summary>
        [Fact(DisplayName = "ValueString allows null and empty values.")]
        public void ValueStringAllowsNullAndEmptyValues()
        {
            var vNull = new ValueString(null);
            Assert.NotNull(vNull);
            Assert.Null(vNull.ToString());
            Assert.Equal(vNull, null); // Implicit conversion from string.
            Assert.True((vNull as object).Equals(null)); // Overridden Equals(object) supports null values.
        }

        /// <summary>
        ///     Tests whether the <see cref="ValueString" />
        ///     supports all parsing methods.
        /// </summary>
        [Fact(DisplayName = "ValueString supports all parsing methods.")]
        public void ValueStringSupportsAllParsingMethods()
        {
            var number = 1.5;
            var date = DateTime.Today;

            // T Parse(string, IFormatProvider) method.
            Test<FormattableMock, double>(number);

            // bool TryParse(string, IFormatProvider, out T) method.
            Test<SafeFormattableMock, double>(number);

            // bool TryParse(string, NumberStyles, IFormatProvider, out T) method.
            Test<SafeNumericMock, double>(number);

            // bool TryParse(string, IFormatProvider, DateTimeStyles, out T) method.
            Test<SafeDateMock, DateTime>(date);

            // [TypeConverter] via TypeDescriptor.GetConverter.
            Test<TypeConverterMock, double>(number);

            /*
             * The rest of the types do not declare a parsing method that accepts
             * a format provider. So they use the current thread's culture
             * when they need culture-specific parsing information.
             * This is not a good practice, since a type that requires
             * culture-specific formatting information to parse a string to an
             * instance of itself is expected to declare a Parse/TryParse
             * overload that accepts an IFormatProvider instance by convention.
             * So if T implements the IFormattable interface,
             * `T.Parse(t.ToString(null, f), f)` should be equal to `t`.
             */

            // Since the ValueString constructor uses the invariant culture when
            // possible, the rest of the types can only be deserialized correctly
            // if the number formatting information supplied by the parsing thread's
            // culture complies with the one supplied by the invariant culture.
            // The invariant culture uses dot (.) as the decimal separator but
            // the "fr-FR" culture uses comma (,) instead. That's why the following
            // tests throw invalid cast exceptions.
            CultureInfo.CurrentCulture = frCulture;

            // T Parse(string) method.
            Assert.Throws<InvalidCastException>(() => Test<PlainMock, double>(number));

            // bool TryParse(string, out T) method.
            Assert.Throws<InvalidCastException>(() => Test<SafeMock, double>(number));

            // T(string) constructor.
            Assert.Throws<InvalidCastException>(() => Test<ConstructorMock, double>(number));

            // "en-US" culture, however, uses dot (.) as the decimal separator just
            // like the invariant culture. That's why the following tests pass.
            CultureInfo.CurrentCulture = usCulture;

            // T Parse(string) method.
            Test<PlainMock, double>(number);

            // bool TryParse(string, out T) method.
            Test<SafeMock, double>(number);

            // T(string) constructor.
            Test<ConstructorMock, double>(number);
        }

        /// <summary>
        ///     Tests whether the dictionary extensions in <see cref="ValueStringUtils" />
        ///     validate their arguments.
        /// </summary>
        [Fact(DisplayName = "Dictionary extensions validate their arguments.")]
        public void DictionaryExtensionsWork()
        {
            var dict = null as Dictionary<string, ValueString>;
            Assert.Throws<ArgumentNullException>("source", () => dict.TryGetValue("Rate", out double value));
            Assert.Throws<ArgumentNullException>("source", () => dict.TryGetValue("Rate", null, out double value));
            Assert.Throws<ArgumentNullException>("target", () => dict.Add("Rate", .1));
            Assert.Throws<ArgumentNullException>("target", () => dict.Set("Rate", .1));

            dict = new Dictionary<string, ValueString>();
            Assert.Throws<ArgumentNullException>("key", () => dict.TryGetValue(null, out double value));
            Assert.Throws<ArgumentNullException>("key", () => dict.TryGetValue(null, null, out double value));
            Assert.Throws<ArgumentNullException>("key", () => dict.Add(null, .1));
            Assert.Throws<ArgumentNullException>("key", () => dict.Set(null, .1));
        }

        /// <summary>
        ///     Tests whether the dictionary extensions in <see cref="ValueStringUtils" />
        ///     use the specified format providers and fallback to the
        ///     invariant culture when no format provider is specified.
        /// </summary>
        [Fact(DisplayName = "Dictionary extensions use the right format providers.")]
        public void DictionaryExtensionsUseRightFormatProviders()
        {
            var dict = new Dictionary<string, ValueString>();

            // Add and set methods should be converted using the invariant
            // culture despite the current culture being "fr-FR".
            CultureInfo.CurrentCulture = frCulture;

            dict.Add("A", .1);
            Assert.Equal("0.1", dict["A"]);

            dict.Set("B", .2);
            Assert.Equal("0.2", dict["B"]);

            // TryGetValue overloads should convert the value using the invariant
            // culture unless another format provider is specified.
            Assert.True(dict.TryGetValue("A", out double a));
            Assert.Equal(.1, a);

            Assert.True(dict.TryGetValue("A", usCulture, out a));
            Assert.Equal(.1, a);

            Assert.True(dict.TryGetValue("B", out double b));
            Assert.Equal(.2, b);

            Assert.False(dict.TryGetValue("B", frCulture, out b));
            Assert.Equal(0, b);
        }

        /// <summary>
        ///     Tests whether the specified type can be initialized from string.
        /// </summary>
        /// <typeparam name="TTarget">Type to initialize from string.</typeparam>
        /// <typeparam name="TValue">Type of the target type's value.</typeparam>
        /// <param name="value">
        ///     The value to initialize a <see cref="ValueString" /> with
        ///     and test whether a <typeparamref name="TTarget" />
        ///     instance can be initialized from a string.
        /// </param>
        private static void Test<TTarget, TValue>(TValue value)
            where TTarget : BaseMock<TValue>
        {
            var v = new ValueString(value);
            Assert.Equal(value, v.As<TTarget>().Value);
            Assert.True(v.Is(out TTarget temp));
            Assert.Equal(value, temp.Value);
        }

        #endregion Methods

        #region Classes

#pragma warning disable SA1600 // Elements must be documented

        private abstract class BaseMock<T>
        {
            public T Value { get; protected set; }
        }

        private sealed class FormattableMock : BaseMock<double>
        {
            public static FormattableMock Parse(string s, IFormatProvider provider)
                => new FormattableMock { Value = double.Parse(s, provider) };
        }

        private sealed class PlainMock : BaseMock<double>
        {
            public static PlainMock Parse(string s)
                => new PlainMock { Value = double.Parse(s) };
        }

        private sealed class SafeFormattableMock : BaseMock<double>
        {
            public static bool TryParse(
                string s, IFormatProvider provider, out SafeFormattableMock result)
            {
                if (double.TryParse(s, NumberStyles.Number, provider, out var number))
                {
                    result = new SafeFormattableMock { Value = number };
                    return true;
                }

                result = null;
                return false;
            }
        }

        private sealed class SafeNumericMock : BaseMock<double>
        {
            public static bool TryParse(
                string s,
                NumberStyles styles,
                IFormatProvider provider,
                out SafeNumericMock result)
            {
                if (double.TryParse(s, styles, provider, out var number))
                {
                    result = new SafeNumericMock { Value = number };
                    return true;
                }

                result = null;
                return false;
            }
        }

        private sealed class SafeDateMock : BaseMock<DateTime>
        {
            public static bool TryParse(
                string s,
                IFormatProvider provider,
                DateTimeStyles styles,
                out SafeDateMock result)
            {
                if (DateTime.TryParse(s, provider, styles, out var date))
                {
                    result = new SafeDateMock { Value = date };
                    return true;
                }

                result = null;
                return false;
            }
        }

        private sealed class SafeMock : BaseMock<double>
        {
            public static bool TryParse(string s, out SafeMock result)
            {
                if (double.TryParse(s, out var number))
                {
                    result = new SafeMock { Value = number };
                    return true;
                }

                result = null;
                return false;
            }
        }

        [TypeConverter(typeof(TestTypeConverter))]
        private sealed class TypeConverterMock : BaseMock<double>
        {
            private sealed class TestTypeConverter : TypeConverter
            {
                public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                    => sourceType == typeof(string);

                public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                {
                    switch (value)
                    {
                        case string s:
                            return new TypeConverterMock { Value = double.Parse(s, culture) };
                        default:
                            return base.ConvertFrom(context, culture, value);
                    }
                }
            }
        }

        private sealed class ConstructorMock : BaseMock<double>
        {
            public ConstructorMock(string s)
                => this.Value = double.Parse(s);
        }

#pragma warning restore SA1600 // Elements must be documented

        #endregion Classes
    }
}
