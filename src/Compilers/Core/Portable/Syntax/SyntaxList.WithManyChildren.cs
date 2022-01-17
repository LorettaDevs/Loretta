// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax
{
    internal partial class SyntaxList
    {
        internal class WithManyChildren : SyntaxList
        {
            private readonly ArrayElement<SyntaxNode?>[] _children;

            internal WithManyChildren(InternalSyntax.SyntaxList green, SyntaxNode? parent, int position)
                : base(green, parent, position)
            {
                _children = new ArrayElement<SyntaxNode?>[green.SlotCount];
            }

            internal override SyntaxNode? GetNodeSlot(int index) =>
                GetRedElement(ref _children[index].Value, index);

            internal override SyntaxNode? GetCachedSlot(int index) =>
                _children[index];
        }
    }
}
