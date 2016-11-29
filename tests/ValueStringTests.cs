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
            StringComparison c = StringComparison.OrdinalIgnoreCase;

            var s1 = "a";
            var v1 = new ValueString(s1);

            Assert.Same(s1, v1.ToString());
            Assert.Same(s1, v1.As<string>());

            var s2 = "A";
            var v2 = new ValueString(s2);

            Assert.Equal(s1 == s2, v1 == v2);
            Assert.Equal(s1.Equals(s2), v1.Equals(v2));
            Assert.Equal(s1.Equals(s2, c), v1.Equals(v2, c));

            s2 = null;
            v2 = new ValueString(s2);

            Assert.True(v2 == null); // null is implicitly converted to ValueString.
            Assert.Same(null, v2.ToString());
            Assert.NotSame(null, v2);
            Assert.Equal(null, v2);
        }

        /// <summary>
        ///     Tests whether the <see cref="ValueString" />
        ///     chooses the right parsing method for each type.
        /// </summary>
        [Fact(DisplayName = nameof(ValueString) + " chooses the right parsing methods.")]
        public void ValueStringChoosesTheRightParsingMethods()
        {
            // The order for As methods:
            // 1. T Parse(string, IFormatProvider)
            // 2. T Parse(string)
            // 3. bool TryParse(string, IFormatProvider, out T)
            // 4. bool TryParse(string, NumberStyles, IFormatProvider, out T)
            // 5. bool TryParse(string, IFormatProvider, DateTimeStyles, out T)
            // 6. bool TryParse(string, out T)
            // 7. T(string)

            // The order for Is methods
            // 1. bool TryParse(string, IFormatProvider, out T)
            // 2. bool TryParse(string, NumberStyles, IFormatProvider, out T)
            // 3. bool TryParse(string, IFormatProvider, DateTimeStyles, out T)
            // 4. bool TryParse(string, out T)
            // 5. T Parse(string, IFormatProvider)
            // 6. T Parse(string)
            // 7. T(string)

            // TODO: Create custom types and test the order.
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
