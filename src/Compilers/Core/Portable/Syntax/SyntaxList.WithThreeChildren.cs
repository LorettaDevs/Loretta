// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax
{
    internal partial class SyntaxList
    {
        internal class WithThreeChildren : SyntaxList
        {
            private SyntaxNode? _child0;
            private SyntaxNode? _child1;
            private SyntaxNode? _child2;

            internal WithThreeChildren(InternalSyntax.SyntaxList green, SyntaxNode? parent, int position)
                : base(green, parent, position)
            {
            }

            internal override SyntaxNode? GetNodeSlot(int index)
            {
                return index switch
                {
                    0 => GetRedElement(ref _child0, 0),
                    1 => GetRedElementIfNotToken(ref _child1),
                    2 => GetRedElement(ref _child2, 2),
                    _ => null,
                };
            }

            internal override SyntaxNode? GetCachedSlot(int index)
            {
                return index switch
                {
                    0 => _child0,
                    1 => _child1,
                    2 => _child2,
                    _ => null,
                };
            }
        }
    }
}
