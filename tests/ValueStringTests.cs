// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Dawn.Tests
{
    using System;
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
        [Fact(DisplayName = nameof(ValueString) + " constructor uses invariant culture.")]
        public void ValueStringConstructorUsesInvariantCulture()
        {
            CultureInfo.DefaultThreadCurrentCulture = frCulture;

            var d = 1.5;
            Assert.Equal("1,5", d.ToString()); // Comma-separated.

            var v = new ValueString(d);
            Assert.Equal("1.5", v.ToString()); // Dot-separated.
        }

        /// <summary>
        ///     Tests whether the <see cref="ValueString" /> methods use the
        ///     invariant culture by default when converting the string data.
        /// </summary>
        [Fact(DisplayName = nameof(ValueString) + " methods use invariant culture.")]
        public void ValueStringMethodsUseInvariantCultureByDefault()
        {
            var time = new DateTime(2016, 11, 28, 22, 59, 58);

            DateTime parsed;

            var fr = new ValueString(time.ToString(frCulture));
            Assert.Equal("28/11/2016 22:59:58", fr.ToString());
            Assert.Throws<InvalidCastException>(() => fr.As<DateTime>());
            Assert.Equal(time, fr.As<DateTime>(frCulture));
            Assert.False(fr.Is(out parsed));
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
        [Fact(DisplayName = nameof(ValueString) + " is equatable.")]
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

            string temp;
            Assert.True(v1a.Is(out temp));
            Assert.Same(s1, temp);
            Assert.True(v1a.Is<string>(out temp));
            Assert.Same(s1, temp);

            // Implements IEquatable<ValueString>.
            var ve = Assert.IsAssignableFrom<IEquatable<ValueString>>(v1a);

            Assert.True(ve.Equals(v1b));
            Assert.False(ve.Equals(v2));

            Assert.True(ve.Equals(s1)); // Implicit conversion from string.
            Assert.False(ve.Equals(s2));

            Assert.True((v1a as object).Equals(v1b)); // Equals(object) handles ValueString.
            Assert.False((v2 as object).Equals(v1b));

            // Implements IEquatable<string>.
            var se = Assert.IsAssignableFrom<IEquatable<string>>(v1a);

            Assert.True(se.Equals(s1));
            Assert.False(se.Equals(s2));

            Assert.True((v1a as object).Equals(s1)); // Equals(object) handles string.
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

            // Convert and parse.
            Assert.True(v1a.Is(s1));
            Assert.False(v1a.Is(s2));

            v1a = new ValueString(1);
            Assert.True(v1a.Is(1));
            Assert.False(v1a.Is(2));
        }

        /// <summary>
        ///     Tests whether <see cref="ValueString" />
        ///     allows <c>null</c> or empty values.
        /// </summary>
        [Fact(DisplayName = nameof(ValueString) + " allows null and empty values.")]
        public void ValueStringAllowsNullAndEmptyValues()
        {
            var vNull = new ValueString(null);
            Assert.NotNull(vNull);
            Assert.Null(vNull.ToString());
            Assert.Equal(vNull, null); // Implicit conversion from string.

            var vEmpty = new ValueString(string.Empty);
            Assert.Empty(vEmpty.ToString());
            Assert.Equal(vEmpty, string.Empty); // Implicit conversion from string.
        }

        /// <summary>
        ///     Tests whether the <see cref="ValueString" />
        ///     supports any parsing method.
        /// </summary>
        [Fact(DisplayName = nameof(ValueString) + " supports any parsing method.")]
        public void ValueStringSupportsAnyParsingMethod()
        {
            CultureInfo.DefaultThreadCurrentCulture = frCulture;

            var number = 1.5;
            var date = DateTime.Today;

            // T Parse(string, IFormatProvider) method.
            Test<TestValuePF, double>(number);

            // bool TryParse(string, IFormatProvider, out T) method.
            Test<TestValueTPF, double>(number);

            // bool TryParse(string, NumberStyles, IFormatProvider, out T) method.
            Test<TestValueTPN, double>(number);

            // bool TryParse(string, IFormatProvider, DateTimeStyles, out T) method.
            Test<TestValueTPD, DateTime>(date);

            // The rest of the types do not accept a format provider,
            // so the invariant culture is not specified implicitly.
            // They use the current thread's culture.
            CultureInfo.DefaultThreadCurrentCulture = usCulture;

            // T Parse(string) method.
            Test<TestValueP, double>(number);

            // bool TryParse(string, out T) method.
            Test<TestValueTP, double>(number);

            // T(string) constructor.
            Test<TestValueC, double>(number);
        }

        /// <summary>
        ///     Tests whether the specified type can be initialied from string.
        /// </summary>
        /// <typeparam name="TTarget">Type to initialize from string.</typeparam>
        /// <typeparam name="TValue">Type of the target type's value.</typeparam>
        /// <param name="value">
        ///     The value to initialize a <see cref="ValueString" /> with
        ///     and test whether a <typeparamref name="TTarget" />
        ///     instance can be initialized from a string.
        /// </param>
        private static void Test<TTarget, TValue>(TValue value)
            where TTarget : TestValue<TValue>
        {
            var v = new ValueString(value);
            Assert.Equal(value, v.As<TTarget>().Value);

            TTarget temp;
            Assert.True(v.Is(out temp));
            Assert.Equal(value, temp.Value);
        }

        #endregion Methods

        #region Classes

#pragma warning disable SA1600 // Elements must be documented

        private abstract class TestValue<T>
        {
            public T Value { get; protected set; }
        }

        private sealed class TestValuePF : TestValue<double>
        {
            public static TestValuePF Parse(string s, IFormatProvider provider)
                => new TestValuePF { Value = double.Parse(s, provider) };
        }

        private sealed class TestValueP : TestValue<double>
        {
            public static TestValueP Parse(string s)
                => new TestValueP { Value = double.Parse(s) };
        }

        private sealed class TestValueTPF : TestValue<double>
        {
            public static bool TryParse(
                string s, IFormatProvider provider, out TestValueTPF result)
            {
                double number;
                if (double.TryParse(s, NumberStyles.Number, provider, out number))
                {
                    result = new TestValueTPF { Value = number };
                    return true;
                }

                result = null;
                return false;
            }
        }

        private sealed class TestValueTPN : TestValue<double>
        {
            public static bool TryParse(
                string s,
                NumberStyles styles,
                IFormatProvider provider,
                out TestValueTPN result)
            {
                double number;
                if (double.TryParse(s, styles, provider, out number))
                {
                    result = new TestValueTPN { Value = number };
                    return true;
                }

                result = null;
                return false;
            }
        }

        private sealed class TestValueTPD : TestValue<DateTime>
        {
            public static bool TryParse(
                string s,
                IFormatProvider provider,
                DateTimeStyles styles,
                out TestValueTPD result)
            {
                DateTime date;
                if (DateTime.TryParse(s, provider, styles, out date))
                {
                    result = new TestValueTPD { Value = date };
                    return true;
                }

                result = null;
                return false;
            }
        }

        private sealed class TestValueTP : TestValue<double>
        {
            public static bool TryParse(string s, out TestValueTP result)
            {
                double number;
                if (double.TryParse(s, out number))
                {
                    result = new TestValueTP { Value = number };
                    return true;
                }

                result = null;
                return false;
            }
        }

        private sealed class TestValueC : TestValue<double>
        {
            public TestValueC(string s)
            {
                this.Value = double.Parse(s);
            }
        }

#pragma warning restore SA1600 // Elements must be documented

        #endregion Classes
    }
}
