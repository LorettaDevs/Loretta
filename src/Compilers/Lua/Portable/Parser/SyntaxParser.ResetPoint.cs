// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
                this.ResetCount = resetCount;
                this.Position = position;
                this.PrevTokenTrailingTrivia = prevTokenTrailingTrivia;
            }
        }
    }
}
