// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#if BINARY_SERIALIZATION

namespace Dawn
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;

    /// <content>Contains the <see cref="ISerializable" /> implementation.</content>
    [Serializable]
    public partial struct ValueString : ISerializable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueString" /> struct
        ///     using the specified deserialization context.
        /// </summary>
        /// <param name="info">The data needed to serialize the value string.</param>
        /// <param name="context">The streaming context.</param>
        private ValueString(SerializationInfo info, StreamingContext context)
            => this.value = info.GetString(nameof(this.value));

        /// <inheritdoc />
        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            => info.AddValue(nameof(this.value), this.value);
    }
}

#endif
