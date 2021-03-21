// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Specifies the options for how generics are displayed in the description of a symbol.
    /// </summary>
    [Flags]
    public enum ObjectDisplayOptions
    {
        /// <summary>
        /// Format object using default options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Whether or not to display integral literals in hexadecimal.
        /// </summary>
        UseHexadecimalNumbers = 1 << 0,

        /// <summary>
        /// Whether or not to quote string literals.
        /// </summary>
        UseQuotes = 1 << 1,

        /// <summary>
        /// Replace non-printable (e.g. control) characters with dedicated (e.g. \t) or unicode (\u0001) escape sequences.
        /// </summary>
        EscapeNonPrintableCharacters = 1 << 2,

        /// <summary>
        /// Escapes characters using their UTF8 encoding instead of unicode escapes.
        /// </summary>
        EscapeWithUtf8 = 1 << 3,
    }
}
