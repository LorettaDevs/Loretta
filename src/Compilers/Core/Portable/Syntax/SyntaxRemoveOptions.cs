// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// The option flags for the RemoveNode(s) methods.
    /// </summary>
    [Flags]
    public enum SyntaxRemoveOptions
    {
        /// <summary>
        /// None of the trivia associated with the node or token is kept.
        /// </summary>
        KeepNoTrivia = 0x0,

        /// <summary>
        /// The leading trivia associated with the node or token is kept.
        /// </summary>
        KeepLeadingTrivia = 0x1,

        /// <summary>
        /// The trailing trivia associated with the node or token is kept.
        /// </summary>
        KeepTrailingTrivia = 0x2,

        /// <summary>
        /// The leading and trailing trivia associated with the node or token is kept.
        /// </summary>
        KeepExteriorTrivia = KeepLeadingTrivia | KeepTrailingTrivia,

        /// <summary>
        /// Any directives that would become unbalanced are kept.
        /// </summary>
        KeepUnbalancedDirectives = 0x4,

        /// <summary>
        /// All directives are kept
        /// </summary>
        KeepDirectives = 0x8,

        /// <summary>
        /// Ensure that at least one EndOfLine trivia is kept if one was present 
        /// </summary>
        KeepEndOfLine = 0x10,

        /// <summary>
        /// Adds elastic marker trivia
        /// </summary>
        AddElasticMarker = 0x20
    }
}
