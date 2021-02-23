using System;
using System.Collections.Generic;
using System.Text;
using Loretta.CodeAnalysis.Syntax.InternalSyntax;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal static class LuaSyntaxNodeCache
    {
        public static GreenNode? TryGetNode(int kind, GreenNode child1, out int hash)
            => SyntaxNodeCache.TryGetNode(kind, child1, out hash);

        public static GreenNode? TryGetNode(int kind, GreenNode child1, GreenNode child2, out int hash)
            => SyntaxNodeCache.TryGetNode(kind, child1, child2, out hash);

        public static GreenNode? TryGetNode(int kind, GreenNode child1, GreenNode child2, GreenNode child3, out int hash)
            => SyntaxNodeCache.TryGetNode(kind, child1, child2, child3, out hash);
    }
}
