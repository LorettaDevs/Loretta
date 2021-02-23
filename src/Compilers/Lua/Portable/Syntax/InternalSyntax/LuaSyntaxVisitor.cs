using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal abstract partial class LuaSyntaxVisitor<TResult>
    {
        public virtual TResult Visit(LuaSyntaxNode node)
            => node is null ? default : node.Accept(this);
    }
}
