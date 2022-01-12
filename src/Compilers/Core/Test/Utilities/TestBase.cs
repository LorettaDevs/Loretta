// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.CodeAnalysis.Text;

namespace Loretta.Test.Utilities
{
    /// <summary>
    /// Base class for all unit test classes.
    /// </summary>
    public abstract class TestBase
    {
        protected TestBase()
        {
        }

        #region Diagnostics

        internal static DiagnosticDescription Diagnostic(
            object code,
            string squiggledText = null,
            object[] arguments = null,
            LinePosition? startLocation = null,
            Func<SyntaxNode, bool> syntaxNodePredicate = null,
            bool argumentOrderDoesNotMatter = false,
            bool isSuppressed = false)
        {
            return TestHelpers.Diagnostic(
                code,
                squiggledText,
                arguments,
                startLocation,
                syntaxNodePredicate,
                argumentOrderDoesNotMatter,
                isSuppressed: isSuppressed);
        }

        internal static DiagnosticDescription Diagnostic(
           object code,
           XCData squiggledText,
           object[] arguments = null,
           LinePosition? startLocation = null,
           Func<SyntaxNode, bool> syntaxNodePredicate = null,
           bool argumentOrderDoesNotMatter = false,
           bool isSuppressed = false)
        {
            return TestHelpers.Diagnostic(
                code,
                squiggledText,
                arguments,
                startLocation,
                syntaxNodePredicate,
                argumentOrderDoesNotMatter,
                isSuppressed: isSuppressed);
        }

        #endregion
    }
}
