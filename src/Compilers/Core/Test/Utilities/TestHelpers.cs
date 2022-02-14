// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Diagnostics;
using System.Xml.Linq;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.CodeAnalysis.Text;

namespace Loretta.Test.Utilities
{
    public static class TestHelpers
    {
        public static DiagnosticDescription Diagnostic(
            object code,
            string squiggledText = null,
            object[] arguments = null,
            LinePosition? startLocation = null,
            Func<SyntaxNode, bool> syntaxNodePredicate = null,
            bool argumentOrderDoesNotMatter = false,
            bool isSuppressed = false)
        {
            Debug.Assert(code is CodeAnalysis.Lua.ErrorCode or int or string);

            return new DiagnosticDescription(
                code as string ?? (object) (int) code,
                false,
                squiggledText,
                arguments,
                startLocation,
                syntaxNodePredicate,
                argumentOrderDoesNotMatter,
                code.GetType(),
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
            return Diagnostic(
                code,
                NormalizeNewLines(squiggledText),
                arguments,
                startLocation,
                syntaxNodePredicate,
                argumentOrderDoesNotMatter,
                isSuppressed: isSuppressed);
        }

        public static string NormalizeNewLines(XCData data)
        {
            if (ExecutionConditionUtil.IsWindows)
            {
                return data.Value.Replace("\n", "\r\n");
            }

            return data.Value;
        }
    }
}
