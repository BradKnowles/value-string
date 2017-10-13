// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Dawn
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     Provides utility extensions for <see cref="ValueString" /> collections.
    /// </summary>
    public static class ValueStringUtils
    {
        #region Methods

        /// <summary>
        ///      Converts the value that is associated with
        ///      the specified key to the specified type,
        ///      using the invariant culture.
        /// </summary>
        /// <typeparam name="TKey">
        ///     The type of keys in the source dictionary.
        /// </typeparam>
        /// <typeparam name="TValue">
        ///     The type to convert the value to.
        /// </typeparam>
        /// <param name="source">The source dictionary.</param>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified
        ///     key, converted to the specified type, if the key is found and the
        ///     conversion is successful; otherwise, the default value of
        ///     <typeparamref name="TValue"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="source" /> contains an element
        ///     that has the specified key and its value can be converted
        ///     to <typeparamref name="TValue" />; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or
        ///     <paramref name="key" /> is <c>null</c>.
        /// </exception>
        public static bool TryGetValue<TKey, TValue>(
            this IReadOnlyDictionary<TKey, ValueString> source,
            TKey key,
            out TValue value)
            => source.TryGetValue(key, CultureInfo.InvariantCulture, out value);

        /// <summary>
        ///      Converts the value that is associated with
        ///      the specified key to the specified type,
        ///      using the specified format provider.
        /// </summary>
        /// <typeparam name="TKey">
        ///     The type of keys in the source dictionary.
        /// </typeparam>
        /// <typeparam name="TValue">
        ///     The type to convert the value to.
        /// </typeparam>
        /// <param name="source">The source dictionary.</param>
        /// <param name="key">The key to locate.</param>
        /// <param name="provider">
        ///     An object that supplies culture-specific parsing information.
        /// </param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified
        ///     key, converted to the specified type, if the key is found and the
        ///     conversion is successful; otherwise, the default value of
        ///     <typeparamref name="TValue"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="source" /> contains an element
        ///     that has the specified key and its value can be converted
        ///     to <typeparamref name="TValue" />; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or
        ///     <paramref name="key" /> is <c>null</c>.
        /// </exception>
        public static bool TryGetValue<TKey, TValue>(
            this IReadOnlyDictionary<TKey, ValueString> source,
            TKey key,
            IFormatProvider provider,
            out TValue value)
        {
            try
            {
                if (source.TryGetValue(key, out ValueString v) && v.Is(provider, out value))
                    return true;
            }
            catch (NullReferenceException)
            {
                throw new ArgumentNullException(nameof(source));
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        ///     Adds an element with the provided key
        ///     and value to the specified dictionary.
        /// </summary>
        /// <typeparam name="TKey">
        ///     The type of keys in the target dictionary.
        /// </typeparam>
        /// <param name="target">The target dictionary.</param>
        /// <param name="key">
        ///     The object to use as the key of the element to add.
        /// </param>
        /// <param name="value">
        ///     The object to use as the value of the element to add.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="target" /> or
        ///     <paramref name="key" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="target" /> is read-only or an element with
        ///     the same key already exists in <paramref name="target" />.
        /// </exception>
        /// <remarks>
        ///     Invariant culture will be used converting the value to string
        ///     if its type implements <see cref="IFormattable" />.
        /// </remarks>
        public static void Add<TKey>(
            this IDictionary<TKey, ValueString> target, TKey key, object value)
        {
            try
            {
                target.Add(key, new ValueString(value));
            }
            catch (NullReferenceException)
            {
                throw new ArgumentNullException(nameof(target));
            }
        }

        #endregion Methods
    }
}
