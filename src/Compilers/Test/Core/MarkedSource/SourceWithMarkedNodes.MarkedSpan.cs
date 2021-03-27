// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Loretta.CodeAnalysis.Text;

namespace Loretta.Test.Utilities
{
    internal sealed partial class SourceWithMarkedNodes
    {
        internal struct MarkedSpan
        {
            public readonly TextSpan MarkedSyntax;
            public readonly TextSpan MatchedSpan;
            public readonly string TagName;
            public readonly int SyntaxKind;
            public readonly int Id;
            public readonly int ParentId;

            public MarkedSpan(TextSpan markedSyntax, TextSpan matchedSpan, string tagName, int syntaxKind, int id, int parentId)
            {
                MarkedSyntax = markedSyntax;
                MatchedSpan = matchedSpan;
                TagName = tagName;
                SyntaxKind = syntaxKind;
                Id = id;
                ParentId = parentId;
            }
        }
    }
}
