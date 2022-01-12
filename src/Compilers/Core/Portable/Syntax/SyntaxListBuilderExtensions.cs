// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax
{
    internal static class SyntaxListBuilderExtensions
    {
        public static SyntaxList<SyntaxNode> ToList(this SyntaxListBuilder? builder)
        {
            var listNode = builder?.ToListNode();
            if (listNode is null)
            {
                return default;
            }

            return new SyntaxList<SyntaxNode>(listNode.CreateRed());
        }

        public static SeparatedSyntaxList<TNode> ToSeparatedList<TNode>(this SyntaxListBuilder? builder) where TNode : SyntaxNode
        {
            var listNode = builder?.ToListNode();
            if (listNode is null)
            {
                return default;
            }

            return new SeparatedSyntaxList<TNode>(new SyntaxNodeOrTokenList(listNode.CreateRed(), 0));
        }
    }
}
