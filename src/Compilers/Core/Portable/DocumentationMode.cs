// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Specifies the different documentation comment processing modes.
    /// </summary>
    /// <remarks>
    /// Order matters: least processing to most processing.
    /// </remarks>
    public enum DocumentationMode : byte
    {
        /// <summary>
        /// Treats documentation comments as regular comments.
        /// </summary>
        None = 0,

        /// <summary>
        /// Parses documentation comments as structured trivia, but do not report any diagnostics.
        /// </summary>
        Parse = 1,

        /// <summary>
        /// Parses documentation comments as structured trivia and report diagnostics.
        /// </summary>
        Diagnose = 2,
    }

    internal static partial class DocumentationModeEnumBounds
    {
        internal static bool IsValid(this DocumentationMode value)
        {
            return value is >= DocumentationMode.None and <= DocumentationMode.Diagnose;
        }
    }
}
