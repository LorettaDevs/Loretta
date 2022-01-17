// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal partial class SyntaxParser
    {
        protected struct ResetPoint
        {
            internal readonly int ResetCount;
            internal readonly int Position;
            internal readonly GreenNode PrevTokenTrailingTrivia;

            internal ResetPoint(int resetCount, int position, GreenNode prevTokenTrailingTrivia)
            {
                ResetCount = resetCount;
                Position = position;
                PrevTokenTrailingTrivia = prevTokenTrailingTrivia;
            }
        }
    }
}
