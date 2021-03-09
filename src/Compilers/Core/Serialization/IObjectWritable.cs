// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Loretta.Utilities
{
    /// <summary>
    /// Objects that implement this interface know how to write their contents to an <see cref="ObjectWriter"/>,
    /// so they can be reconstructed later by an <see cref="ObjectReader"/>.
    /// </summary>
    internal interface IObjectWritable
    {
        void WriteTo(ObjectWriter writer);

        /// <summary>
        /// Returns 'true' when the same instance could be used more than once.
        /// Instances that return 'false' should not be tracked for the purpose 
        /// of de-duplication while serializing/deserializing.
        /// </summary>
        bool ShouldReuseInSerialization { get; }
    }
}
