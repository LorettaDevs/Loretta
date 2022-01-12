// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A class with common diagnostic tags.
    /// </summary>
    public static class WellKnownDiagnosticTags
    {
        /// <summary>
        /// Indicates that the diagnostic is related to some unnecessary source code.
        /// </summary>
        public const string Unnecessary = nameof(Unnecessary);

        /// <summary>
        /// Indicates that the diagnostic is related to build.
        /// </summary>
        public const string Build = nameof(Build);

        /// <summary>
        /// Indicates that the diagnostic is reported by the compiler.
        /// </summary>
        public const string Compiler = nameof(Compiler);

        /// <summary>
        /// Indicates that the diagnostic is not configurable, i.e. it cannot be suppressed or filtered or have its severity changed.
        /// </summary>
        public const string NotConfigurable = nameof(NotConfigurable);
    }
}
