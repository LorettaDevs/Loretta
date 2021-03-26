// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Diagnostics;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal class SyntaxLastTokenReplacer : LuaSyntaxRewriter
    {
        private readonly SyntaxToken _oldToken;
        private readonly SyntaxToken _newToken;
        private int _count = 1;
        private bool _found;

        private SyntaxLastTokenReplacer(SyntaxToken oldToken, SyntaxToken newToken)
        {
            _oldToken = oldToken;
            _newToken = newToken;
        }

        internal static TRoot Replace<TRoot>(TRoot root, SyntaxToken newToken)
            where TRoot : LuaSyntaxNode
        {
            var oldToken = root.GetLastToken();
            var replacer = new SyntaxLastTokenReplacer(oldToken, newToken);
            var newRoot = (TRoot) replacer.Visit(root);
            Debug.Assert(replacer._found);
            return newRoot;
        }

        private static int CountNonNullSlots(LuaSyntaxNode node) =>
            node.ChildNodesAndTokens().Count;

        public override LuaSyntaxNode Visit(LuaSyntaxNode node)
        {
            if (node != null && !_found)
            {
                _count--;
                if (_count == 0)
                {
                    if (node is SyntaxToken token)
                    {
                        Debug.Assert(token == _oldToken);
                        _found = true;
                        return _newToken;
                    }

                    _count += CountNonNullSlots(node);
                    return base.Visit(node);
                }
            }

            return node;
        }
    }
}
